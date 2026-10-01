namespace Veimen_API.Models.Dtos;

// Fila del dashboard de consumo de tokens: totales por día (end_date de la traza), node y
// llm_model. Solo considera pasos con node_type = 'LLM'. LlmModel puede ser null si la traza
// no lo registró.
public class ServiceRequestTokenUsageRow
{
    public DateTime Date { get; set; }
    public string Node { get; set; } = string.Empty;
    public string? LlmModel { get; set; }
    public long InputTokens { get; set; }
    public long OutputTokens { get; set; }
    public long TotalTokens { get; set; }
}
