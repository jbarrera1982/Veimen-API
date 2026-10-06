using System.Text.Json.Serialization;

namespace Veimen_API.Models.Dtos;

// Bucket de tiempo dentro de las respuestas de usage/costs de OpenAI (bucket_width = 1d).
public class OpenAiBucket<T>
{
    [JsonPropertyName("object")]
    public string Object { get; set; } = "bucket";

    [JsonPropertyName("start_time")]
    public long StartTime { get; set; }

    [JsonPropertyName("end_time")]
    public long EndTime { get; set; }

    [JsonPropertyName("results")]
    public List<T> Results { get; set; } = [];
}
