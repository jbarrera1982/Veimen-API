using Veimen_API.Helpers;
using Veimen_API.Models;
using Veimen_API.Models.Dtos;
using Veimen_API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Veimen_API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ServiceRequestsController : ControllerBase
{
    private const int MaxPageSize = 100;

    private readonly IServiceRequestService _service;

    public ServiceRequestsController(IServiceRequestService service)
    {
        _service = service;
    }

    [HttpGet]
    [Authorize(Policy = Permissions.ServiceRequestsRead)]
    public async Task<ActionResult<PagedResult<ServiceRequest>>> GetAll([FromQuery] ServiceRequestQueryParams query)
    {
        if (query.Page < 1)
        {
            return BadRequest("El parámetro 'page' debe ser mayor o igual a 1.");
        }

        if (query.PageSize < 1 || query.PageSize > MaxPageSize)
        {
            return BadRequest($"El parámetro 'pageSize' debe estar entre 1 y {MaxPageSize}.");
        }

        if (!DateQueryParser.TryParse(query.StartDate, out var startDate)
            || !DateQueryParser.TryParse(query.EndDate, out var endDate))
        {
            return BadRequest($"Formato de fecha inválido. Use '{DateQueryParser.Format}' (ej: 20260131).");
        }

        if (startDate.HasValue && endDate.HasValue && startDate > endDate)
        {
            return BadRequest("El parámetro 'start_date' no puede ser posterior a 'end_date'.");
        }

        if (!string.IsNullOrWhiteSpace(query.Status)
            && !ServiceRequest.ValidStatuses.Contains(query.Status, StringComparer.Ordinal))
        {
            return BadRequest(
                $"El parámetro 'status' no es válido. Valores permitidos: {string.Join(", ", ServiceRequest.ValidStatuses)}.");
        }

        var result = await _service.GetFilteredAsync(query, startDate, endDate);
        return Ok(result);
    }

    [HttpGet("dashboard")]
    [Authorize(Policy = Permissions.DashboardRead)]
    public async Task<ActionResult<IEnumerable<ServiceRequestDashboardRow>>> GetDashboard(
        [FromQuery(Name = "start_date")] string? startDateRaw,
        [FromQuery(Name = "end_date")] string? endDateRaw)
    {
        if (!DateQueryParser.TryParse(startDateRaw, out var startDate)
            || !DateQueryParser.TryParse(endDateRaw, out var endDate))
        {
            return BadRequest($"Formato de fecha inválido. Use '{DateQueryParser.Format}' (ej: 20260131).");
        }

        if (startDate.HasValue && endDate.HasValue && startDate > endDate)
        {
            return BadRequest("El parámetro 'start_date' no puede ser posterior a 'end_date'.");
        }

        var result = await _service.GetDashboardAsync(startDate, endDate);
        return Ok(result);
    }

    [HttpGet("trace")]
    [Authorize(Policy = Permissions.ServiceRequestsRead)]
    public async Task<ActionResult<IEnumerable<ServiceRequestTraceStep>>> GetTrace(
        [FromQuery(Name = "request_number")] long requestNumber)
    {
        if (requestNumber <= 0)
        {
            return BadRequest("El parámetro 'request_number' debe ser mayor a 0.");
        }

        var result = await _service.GetTraceAsync(requestNumber);
        return Ok(result);
    }
}
