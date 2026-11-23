using System.Text.Json.Serialization;

namespace Veimen_API.Models.Dtos;

// Contrato del webhook n8n /webhook/email-agent. Las claves van en snake_case
// tal como las espera el flujo (message_id, receipt_date, body_html).
public class EmailEvaluationWebhookRequest
{
    [JsonPropertyName("message_id")]
    public string MessageId { get; set; } = string.Empty;

    [JsonPropertyName("receipt_date")]
    public string ReceiptDate { get; set; } = string.Empty;

    [JsonPropertyName("from")]
    public string From { get; set; } = string.Empty;

    [JsonPropertyName("subject")]
    public string Subject { get; set; } = string.Empty;

    [JsonPropertyName("body")]
    public string Body { get; set; } = string.Empty;

    [JsonPropertyName("body_html")]
    public string BodyHtml { get; set; } = string.Empty;
}
