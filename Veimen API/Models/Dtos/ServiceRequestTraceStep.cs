namespace Veimen_API.Models.Dtos;

// Fila de traza de un service request (tabla service_request_trace).
public class ServiceRequestTraceStep
{
    public long TraceId { get; set; }

    public long RequestNumber { get; set; }

    public int Sequence { get; set; }

    public string? Node { get; set; }    

    public string? NodeType { get; set; }

    public string? LlmModel { get; set; }

    public string? PromptVersion { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public long? DurationMs { get; set; }

    public string? Status { get; set; }

    // Se mapea como string por convención del proyecto (igual que Prompt.SchemaOutput).
    public string? Confidence { get; set; }

    // Columnas JSON: MySqlConnector las devuelve como string.
    public string? InputJson { get; set; }

    public string? OutputJson { get; set; }

    public string? Observations { get; set; }

    public DateTime? CreatedAt { get; set; }

    public long? PromptId { get; set; }

    public string? PromptResult { get; set; }
}
