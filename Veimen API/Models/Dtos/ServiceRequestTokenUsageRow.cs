namespace Veimen_API.Models.Dtos;

// Fila del dashboard de consumo de tokens: totales por día (end_date de la traza), agent y node.
// Solo considera pasos con node_type = 'LLM'.
public class ServiceRequestTokenUsageRow
{
    public DateTime Date { get; set; }    
    public string Node { get; set; } = string.Empty;
    public long InputTokens { get; set; }
    public long OutputTokens { get; set; }
    public long TotalTokens { get; set; }
}
