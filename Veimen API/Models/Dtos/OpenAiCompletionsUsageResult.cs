using System.Text.Json.Serialization;

namespace Veimen_API.Models.Dtos;

// Resultado de consumo de completions dentro de un bucket de la API de OpenAI
// (GET /v1/organization/usage/completions). Los campos opcionales son nullables
// según la documentación de OpenAI.
public class OpenAiCompletionsUsageResult
{
    [JsonPropertyName("object")]
    public string Object { get; set; } = "organization.usage.completions.result";

    [JsonPropertyName("input_tokens")]
    public long InputTokens { get; set; }

    [JsonPropertyName("input_cached_tokens")]
    public long? InputCachedTokens { get; set; }

    [JsonPropertyName("input_audio_tokens")]
    public long? InputAudioTokens { get; set; }

    [JsonPropertyName("output_tokens")]
    public long OutputTokens { get; set; }

    [JsonPropertyName("output_audio_tokens")]
    public long? OutputAudioTokens { get; set; }

    [JsonPropertyName("num_model_requests")]
    public long NumModelRequests { get; set; }

    [JsonPropertyName("project_id")]
    public string? ProjectId { get; set; }

    [JsonPropertyName("user_id")]
    public string? UserId { get; set; }

    [JsonPropertyName("api_key_id")]
    public string? ApiKeyId { get; set; }

    [JsonPropertyName("model")]
    public string? Model { get; set; }

    [JsonPropertyName("batch")]
    public bool? Batch { get; set; }
}
