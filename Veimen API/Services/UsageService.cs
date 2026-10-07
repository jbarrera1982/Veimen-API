using System.Net.Http.Headers;
using System.Net.Http.Json;
using Veimen_API.Exceptions;
using Veimen_API.Models.Dtos;

namespace Veimen_API.Services;

// Consulta costos y consumo en la API de OpenAI (/v1/organization/...).
// No usa repositorio: los datos viven en OpenAI, no en la base de datos.
public class UsageService : IUsageService
{
    // Límite máximo de buckets por página que acepta cada endpoint de OpenAI con bucket_width=1d.
    // La paginación recorre todas las páginas, así que rangos largos se completan igual.
    private const string BucketWidth = "1d";
    private const int CostsPageLimit = 180;   // /v1/organization/costs: 1..180
    private const int UsagePageLimit = 31;    // /v1/organization/usage/completions: 1..31

    // Agrupación: API key + la dimensión que muestra el frontend (model en
    // completions, line_item en costs). OpenAI permite combinar group_by; si se
    // agrupara SOLO por api_key_id, model/line_item llegarían null y se perdería
    // el desglose actual de los dashboards (todo caería en la fila '—').
    private const string GroupBy = "api_key_id";
    private const string GroupByModel = "model";
    private const string GroupByLineItem = "line_item";

    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private readonly string _adminApiKey;

    public UsageService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _baseUrl = (configuration["OpenAI:BaseUrl"] ?? "https://api.openai.com").TrimEnd('/');
        _adminApiKey = configuration["OpenAI:AdminApiKey"] ?? string.Empty;
    }

    public Task<OpenAiPage<OpenAiCostResult>> GetCostsAsync(DateTime startDate, DateTime? endDate)
        => GetAllPagesAsync<OpenAiCostResult>("costs", CostsPageLimit, startDate, endDate, GroupBy, GroupByLineItem);

    public Task<OpenAiPage<OpenAiCompletionsUsageResult>> GetCompletionsUsageAsync(DateTime startDate, DateTime? endDate)
        => GetAllPagesAsync<OpenAiCompletionsUsageResult>("usage/completions", UsagePageLimit, startDate, endDate, GroupBy, GroupByModel);

    private async Task<OpenAiPage<T>> GetAllPagesAsync<T>(
        string path, int pageLimit, DateTime startDate, DateTime? endDate, params string[] groupBy)
    {
        if (string.IsNullOrWhiteSpace(_adminApiKey))
        {
            throw new OpenAiException(
                "'OpenAI:AdminApiKey' no está configurado. " +
                "Configúrelo en User Secrets (dev) o en la variable de entorno 'OpenAI__AdminApiKey' (prod).",
                StatusCodes.Status500InternalServerError);
        }

        // start_time es inclusivo; end_date llega inclusivo (convención del proyecto) y OpenAI
        // espera end_time exclusivo, por eso se envía la medianoche UTC del día siguiente.
        var startTime = ToUnixSeconds(startDate);
        long? endTime = endDate.HasValue ? ToUnixSeconds(endDate.Value.AddDays(1)) : null;

        // Se recorren todas las páginas de OpenAI para no truncar rangos mayores a pageLimit días.
        var buckets = new List<OpenAiBucket<T>>();
        string? pageCursor = null;
        do
        {
            var page = await GetPageAsync<T>(path, pageLimit, startTime, endTime, pageCursor, groupBy);
            if (page.Data.Count > 0)
            {
                buckets.AddRange(page.Data);
            }

            pageCursor = page.HasMore && !string.IsNullOrEmpty(page.NextPage) ? page.NextPage : null;
        }
        while (pageCursor is not null);

        return new OpenAiPage<T> { Data = buckets };
    }

    private async Task<OpenAiPage<T>> GetPageAsync<T>(
        string path, int pageLimit, long startTime, long? endTime, string? pageCursor, params string[] groupBy)
    {
        // OpenAI recibe group_by como parámetro repetido (en la doc es un array).
        var groupByQuery = string.Join('&', groupBy.Select(g => $"group_by={g}"));
        var query = $"start_time={startTime}&bucket_width={BucketWidth}&limit={pageLimit}&{groupByQuery}";
        if (endTime.HasValue)
        {
            query += $"&end_time={endTime.Value}";
        }
        if (!string.IsNullOrEmpty(pageCursor))
        {
            query += $"&page={Uri.EscapeDataString(pageCursor)}";
        }

        using var request = new HttpRequestMessage(
            HttpMethod.Get, $"{_baseUrl}/v1/organization/{path}?{query}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _adminApiKey);

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.SendAsync(request);
        }
        catch (HttpRequestException ex)
        {
            throw new OpenAiException($"No se pudo conectar con la API de OpenAI: {ex.Message}",
                StatusCodes.Status502BadGateway);
        }
        catch (TaskCanceledException)
        {
            throw new OpenAiException("La API de OpenAI no respondió a tiempo (timeout).",
                StatusCodes.Status504GatewayTimeout);
        }

        using (response)
        {
            if (!response.IsSuccessStatusCode)
            {
                var detail = await response.Content.ReadAsStringAsync();
                if (detail.Length > 300)
                {
                    detail = detail[..300];
                }

                throw new OpenAiException(
                    $"La API de OpenAI respondió con error (HTTP {(int)response.StatusCode}): {detail}",
                    StatusCodes.Status502BadGateway);
            }

            return await response.Content.ReadFromJsonAsync<OpenAiPage<T>>()
                ?? new OpenAiPage<T>();
        }
    }

    // Las fechas de query son fechas planas (convención del proyecto); se interpretan como UTC.
    private static long ToUnixSeconds(DateTime date) =>
        new DateTimeOffset(date.Date, TimeSpan.Zero).ToUnixTimeSeconds();
}
