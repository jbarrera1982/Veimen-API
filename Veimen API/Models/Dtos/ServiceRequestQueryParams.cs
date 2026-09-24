using Microsoft.AspNetCore.Mvc;

namespace Veimen_API.Models.Dtos;

public class ServiceRequestQueryParams
{
    // Formato yyyyMMdd (ej: 20260131). Se parsea con DateQueryParser en el controlador.
    [FromQuery(Name = "start_date")]
    public string? StartDate { get; set; }

    // Formato yyyyMMdd (ej: 20260131). Se parsea con DateQueryParser en el controlador.
    [FromQuery(Name = "end_date")]
    public string? EndDate { get; set; }

    [FromQuery(Name = "status")]
    public string? Status { get; set; }

    // Búsqueda combinada: coincidencia exacta sobre request_number (solo si el valor es numérico)
    // y wildcard (LIKE con substring) sobre `from` y `subject`, escapando %/_/! del input ('!' es el escape de LIKE).
    [FromQuery(Name = "search")]
    public string? Search { get; set; }

    [FromQuery(Name = "page")]
    public int Page { get; set; } = 1;

    [FromQuery(Name = "pageSize")]
    public int PageSize { get; set; } = 20;
}
