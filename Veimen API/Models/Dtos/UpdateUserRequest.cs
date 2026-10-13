using System.ComponentModel.DataAnnotations;

namespace Veimen_API.Models.Dtos;

public class UpdateUserRequest
{
    [EmailAddress]
    [MaxLength(150)]
    public string? Email { get; set; }

    [MaxLength(150)]
    public string? FullName { get; set; }

    public long? ProfileId { get; set; }

    public bool? Active { get; set; }

    // Semántica PATCH: ausente (null) = no cambia; <= 0 = quita el cliente (NULL);
    // > 0 = asigna ese cliente. Se usa 0 (y no null) para poder distinguir "quitar"
    // de "no enviado", ya que otras acciones (activar/desactivar) solo mandan { active }.
    public int? ClientId { get; set; }
}
