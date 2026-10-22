namespace Veimen_API.Models.Dtos;

// Resultado de registrar una revisión manual sobre un requerimiento.
public enum ReviewOutcome
{
    // Revisión registrada: estado actualizado y traza creada.
    Success,

    // No existe el requerimiento con ese request_number.
    NotFound,

    // El requerimiento existe pero su estado actual no es 'Pending review'.
    NotPending,
}
