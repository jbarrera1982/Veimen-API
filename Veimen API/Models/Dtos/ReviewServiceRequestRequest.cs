using System.ComponentModel.DataAnnotations;

namespace Veimen_API.Models.Dtos;

// Cuerpo de POST /api/ServiceRequests/review: revisión manual de un requerimiento
// pendiente. Crea una traza ('Revisión manual') y cambia el estado del requerimiento.
public class ReviewServiceRequestRequest
{
    [Range(1, long.MaxValue, ErrorMessage = "El 'requestNumber' debe ser mayor a 0.")]
    public long RequestNumber { get; set; }

    // Estado destino permitido: solo Rejected o Closed (la validación de pertenencia
    // a ese subconjunto se hace en el controlador/modelo, no con DataAnnotations).
    [Required]
    public string Status { get; set; } = string.Empty;

    [Required(ErrorMessage = "Las 'observations' son obligatorias.")]
    public string Observations { get; set; } = string.Empty;
}
