using System.ComponentModel.DataAnnotations;

namespace Veimen_API.Models.Dtos;

// Cambio del propio idioma: "Español" o "Inglés" (también se aceptan es/en).
public class UpdateLanguageRequest
{
    [Required]
    [MaxLength(100)]
    public string Language { get; set; } = string.Empty;
}
