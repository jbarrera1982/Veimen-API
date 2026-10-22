using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Veimen_API.Exceptions;
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

    // Estados destino válidos de una revisión manual (subconjunto de ValidStatuses).
    private static readonly string[] ReviewStatuses = ["Rejected", "Closed"];

    private readonly IServiceRequestService _service;
    private readonly IAuditService _auditService;

    public ServiceRequestsController(IServiceRequestService service, IAuditService auditService)
    {
        _service = service;
        _auditService = auditService;
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
        [FromQuery(Name = "end_date")] string? endDateRaw,
        [FromQuery(Name = "client_id")] int? clientId)
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

        var result = await _service.GetDashboardAsync(startDate, endDate, clientId);
        return Ok(result);
    }

    [HttpGet("tokens")]
    [Authorize(Policy = Permissions.TokensRead)]
    public async Task<ActionResult<IEnumerable<ServiceRequestTokenUsageRow>>> GetTokenUsage(
        [FromQuery(Name = "start_date")] string? startDateRaw,
        [FromQuery(Name = "end_date")] string? endDateRaw,
        [FromQuery(Name = "client_id")] int? clientId)
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

        var result = await _service.GetTokenUsageAsync(startDate, endDate, clientId);
        return Ok(result);
    }

    [HttpGet("trace")]
    [Authorize(Policy = Permissions.DashboardRead)]
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

    // Dispara el webhook de n8n que genera el informe de auditoría. El frontend ya no
    // conoce la URL del webhook: la petición entra aquí y el backend la reenvía. El email
    // del solicitante se toma del access token, no del cliente (no se puede falsificar).
    [HttpPost("audit")]
    [Authorize(Policy = Permissions.ServiceRequestsRead)]
    public async Task<IActionResult> Audit([FromQuery(Name = "request_number")] long requestNumber)
    {
        if (requestNumber <= 0)
        {
            return BadRequest("El parámetro 'request_number' debe ser mayor a 0.");
        }

        var email = User.FindFirstValue(JwtRegisteredClaimNames.Email)
            ?? User.FindFirstValue(ClaimTypes.Email);

        try
        {
            await _auditService.RequestAuditAsync(requestNumber, email);
            return Ok();
        }
        catch (WebhookException ex)
        {
            return StatusCode(ex.StatusCode, new { message = ex.Message });
        }
    }

    // Revisión manual de un requerimiento 'Pending review': cambia su estado al elegido
    // (Rejected/Closed) y registra el paso 'Revisión manual' en service_request_trace,
    // en una única transacción. El estado destino y las observaciones van en el cuerpo.
    [HttpPost("review")]
    [Authorize(Policy = Permissions.TraceWrite)]
    public async Task<ActionResult> Review([FromBody] ReviewServiceRequestRequest request)
    {
        if (!ReviewStatuses.Contains(request.Status, StringComparer.Ordinal))
        {
            return BadRequest(
                $"El 'status' no es válido. Valores permitidos: {string.Join(", ", ReviewStatuses)}.");
        }

        if (string.IsNullOrWhiteSpace(request.Observations))
        {
            return BadRequest("Las 'observations' son obligatorias.");
        }

        var reviewerName = User.FindFirstValue(ClaimTypes.Name)
            ?? User.FindFirstValue("unique_name")
            ?? User.FindFirstValue("name")
            ?? User.Identity?.Name
            ?? "Sistema";

        var outcome = await _service.ReviewAsync(
            request.RequestNumber,
            request.Status,
            request.Observations,
            reviewerName);

        return outcome switch
        {
            ReviewOutcome.NotFound => NotFound(
                new { message = $"El requerimiento {request.RequestNumber} no existe." }),
            ReviewOutcome.NotPending => Conflict(new
            {
                message =
                    $"El requerimiento {request.RequestNumber} no está pendiente de revisión.",
            }),
            _ => NoContent(),
        };
    }
}
