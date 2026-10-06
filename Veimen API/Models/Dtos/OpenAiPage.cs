using System.Text.Json.Serialization;

namespace Veimen_API.Models.Dtos;

// Página de resultados de las APIs de usage/costs de OpenAI (formato de paginación común).
// Se devuelve al cliente con el mismo formato (snake_case) que entrega OpenAI.
public class OpenAiPage<T>
{
    [JsonPropertyName("object")]
    public string Object { get; set; } = "page";

    [JsonPropertyName("data")]
    public List<OpenAiBucket<T>> Data { get; set; } = [];

    [JsonPropertyName("has_more")]
    public bool HasMore { get; set; }

    [JsonPropertyName("next_page")]
    public string? NextPage { get; set; }
}
