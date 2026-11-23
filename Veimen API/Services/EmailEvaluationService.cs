using System.Net.Http.Json;
using Veimen_API.Exceptions;
using Veimen_API.Helpers;
using Veimen_API.Models;
using Veimen_API.Models.Dtos;

namespace Veimen_API.Services;

// Reenvía un caso del dataset al webhook de n8n (/webhook/email-agent) con el
// mismo cuerpo que el flujo recibe para un correo real. La respuesta se devuelve
// tal cual al frontend; no se persiste historial de ejecuciones.
public class EmailEvaluationService : IEmailEvaluationService
{
    private readonly HttpClient _httpClient;
    private readonly string _webhookUrl;

    public EmailEvaluationService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _webhookUrl = (configuration["N8n:EmailEvaluationWebhookUrl"] ?? string.Empty).Trim();
    }

    public async Task<string> EvaluateAsync(EmailEvaluationDataset dataset)
    {
        if (string.IsNullOrWhiteSpace(_webhookUrl))
        {
            throw new WebhookException(
                "'N8n:EmailEvaluationWebhookUrl' no está configurado. " +
                "Configúrelo en appsettings, User Secrets (dev) o en la variable de entorno " +
                "'N8n__EmailEvaluationWebhookUrl' (prod).",
                StatusCodes.Status500InternalServerError);
        }

        var payload = new EmailEvaluationWebhookRequest
        {
            // message_id único por ejecución: permite repetir el mismo caso sin que
            // n8n lo trate como un correo ya procesado.
            MessageId = $"eval-{dataset.Id}-{Guid.NewGuid():N}",
            ReceiptDate = DateTimeOffset.UtcNow.ToString("yyyy-MM-dd'T'HH:mm:ss.fffzzz"),
            From = dataset.From,
            Subject = dataset.Subject ?? string.Empty,
            Body = HtmlTextConverter.ToPlainText(dataset.Content),
            BodyHtml = dataset.Content ?? string.Empty,
        };

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.PostAsJsonAsync(_webhookUrl, payload);
        }
        catch (HttpRequestException ex)
        {
            throw new WebhookException(
                $"No se pudo conectar con el webhook de evaluación: {ex.Message}",
                StatusCodes.Status502BadGateway);
        }
        catch (TaskCanceledException)
        {
            throw new WebhookException(
                "El webhook de evaluación no respondió a tiempo (timeout).",
                StatusCodes.Status504GatewayTimeout);
        }

        using (response)
        {
            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                if (body.Length > 300)
                {
                    body = body[..300];
                }

                throw new WebhookException(
                    $"El webhook de evaluación respondió con error " +
                    $"(HTTP {(int)response.StatusCode}): {body}",
                    StatusCodes.Status502BadGateway);
            }

            return body;
        }
    }
}
