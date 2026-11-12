using System.ComponentModel.DataAnnotations;

namespace Veimen_API.Models.Dtos;

public class CreateUserRequest
{
    [Required]
    [MaxLength(50)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(150)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(8)]
    [MaxLength(100)]
    public string Password { get; set; } = string.Empty;

    // Teléfono de contacto opcional.
    [MaxLength(30)]
    public string? Phone { get; set; }

    [MaxLength(150)]
    public string? FullName { get; set; }

    [Required]
    public long ProfileId { get; set; }

    // Cliente asociado (client_id). null = sin cliente ("Todos").
    public int? ClientId { get; set; }

    // Idioma preferido: "Español" (por defecto si se omite) o "Inglés".
    [MaxLength(100)]
    public string? Language { get; set; }
}
