using System.Text.Json.Serialization;

namespace Veimen_API.Models.Dtos;

// Monto del costo dentro de un resultado de la API de OpenAI.
public class OpenAiCostAmount
{
    [JsonPropertyName("value")]
    public decimal? Value { get; set; }

    [JsonPropertyName("currency")]
    public string? Currency { get; set; }
}
