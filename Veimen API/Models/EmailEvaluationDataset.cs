using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Veimen_API.Models;

// Caso de prueba del dataset de evaluación de correos.
// `case` y `from` son palabras reservadas en MySQL: se citan con backticks en el SQL.
[Table("email_evaluation_dataset")]
public class EmailEvaluationDataset
{
    // Clave primaria autonumérica (la asigna MySQL al insertar).
    [Column("id")]
    [Key]
    public long Id { get; set; }

    // Nombre del caso: texto libre, editable. Sin restricción de caracteres.
    [Column("case")]
    [Required]
    [MaxLength(255)]
    public string Case { get; set; } = string.Empty;

    [Column("scenario")]
    [Required]
    [MaxLength(150)]
    public string Scenario { get; set; } = string.Empty;

    [Column("from")]
    [Required]
    [MaxLength(320)]
    [EmailAddress]
    public string From { get; set; } = string.Empty;

    [Column("subject")]
    [MaxLength(500)]
    public string? Subject { get; set; }

    // HTML original del correo. Se guarda tal cual y se renderiza sanitizado.
    [Column("content")]
    public string? Content { get; set; }

    [Column("client_id")]
    [Range(1, int.MaxValue, ErrorMessage = "El cliente es obligatorio.")]
    public int ClientId { get; set; }

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    [Column("created_by")]
    [MaxLength(100)]
    public string? CreatedBy { get; set; }

    [Column("updated_by")]
    [MaxLength(100)]
    public string? UpdatedBy { get; set; }
}
