using Microsoft.AspNetCore.Mvc;

namespace Veimen_API.Models.Dtos;

public class EmailEvaluationQueryParams
{
    // Filtro por cliente. Ausente = sin filtro ("Todos") para usuarios sin cliente asociado.
    [FromQuery(Name = "client_id")]
    public int? ClientId { get; set; }

    // Filtro exacto por escenario (los valores se obtienen de /scenarios).
    [FromQuery(Name = "scenario")]
    public string? Scenario { get; set; }

    // Búsqueda parcial sobre scenario, from y subject.
    [FromQuery(Name = "search")]
    public string? Search { get; set; }

    [FromQuery(Name = "page")]
    public int Page { get; set; } = 1;

    [FromQuery(Name = "pageSize")]
    public int PageSize { get; set; } = 20;
}
