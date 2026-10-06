using System.Text.Json.Serialization;

namespace Veimen_API.Models.Dtos;

// Resultado de costo dentro de un bucket de la API de OpenAI.
// Todos los campos son opcionales/nullables según la documentación de OpenAI.
public class OpenAiCostResult
{
    [JsonPropertyName("object")]
    public string Object { get; set; } = "organization.costs.result";

    [JsonPropertyName("amount")]
    public OpenAiCostAmount? Amount { get; set; }

    [JsonPropertyName("line_item")]
    public string? LineItem { get; set; }

    [JsonPropertyName("project_id")]
    public string? ProjectId { get; set; }

    [JsonPropertyName("api_key_id")]
    public string? ApiKeyId { get; set; }

    [JsonPropertyName("quantity")]
    public decimal? Quantity { get; set; }

    [JsonPropertyName("quantity_unit")]
    public string? QuantityUnit { get; set; }
}
