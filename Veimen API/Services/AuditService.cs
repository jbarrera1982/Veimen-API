using Veimen_API.Exceptions;

namespace Veimen_API.Services;

// Llama al webhook de n8n que genera el informe de auditoría. El frontend ya no conoce
// esta URL: la petición pasa por la API y el backend reenvía el email del usuario del token.
public class AuditService : IAuditService
{
    private readonly HttpClient _httpClient;
    private readonly string _webhookUrl;

    public AuditService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _webhookUrl = (configuration["N8n:AuditWebhookUrl"] ?? string.Empty).Trim();
    }

    public async Task RequestAuditAsync(long requestNumber, string? email)
    {
        if (string.IsNullOrWhiteSpace(_webhookUrl))
        {
            throw new WebhookException(
                "'N8n:AuditWebhookUrl' no está configurado. " +
                "Configúrelo en appsettings, User Secrets (dev) o la variable de entorno 'N8n__AuditWebhookUrl' (prod).",
                StatusCodes.Status500InternalServerError);
        }

        // Misma convención que usaba el frontend: los datos viajan como query string.
        var query = $"request_number={requestNumber}";
        if (!string.IsNullOrWhiteSpace(email))
        {
            query += $"&email={Uri.EscapeDataString(email)}";
        }

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.PostAsync($"{_webhookUrl}?{query}", null);
        }
        catch (HttpRequestException ex)
        {
            throw new WebhookException($"No se pudo conectar con el webhook de auditoría: {ex.Message}",
                StatusCodes.Status502BadGateway);
        }
        catch (TaskCanceledException)
        {
            throw new WebhookException("El webhook de auditoría no respondió a tiempo (timeout).",
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

                throw new WebhookException(
                    $"El webhook de auditoría respondió con error (HTTP {(int)response.StatusCode}): {detail}",
                    StatusCodes.Status502BadGateway);
            }
        }
    }
}
