namespace Veimen_API.Models.Dtos;

// Fila del listado paginado. No incluye el HTML completo: el repositorio trae un
// recorte de `content` y el servicio lo convierte en texto plano para el preview.
public class EmailEvaluationCaseListItem
{
    public long Id { get; set; }

    public string Scenario { get; set; } = string.Empty;

    public string From { get; set; } = string.Empty;

    public string? Subject { get; set; }

    public string ContentPreview { get; set; } = string.Empty;

    public int ClientId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
