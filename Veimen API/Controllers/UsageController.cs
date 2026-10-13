using Veimen_API.Exceptions;
using Veimen_API.Helpers;
using Veimen_API.Models.Dtos;
using Veimen_API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Veimen_API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsageController : ControllerBase
{
    private readonly IUsageService _service;
    private readonly IClientService _clientService;

    public UsageController(IUsageService service, IClientService clientService)
    {
        _service = service;
        _clientService = clientService;
    }

    [HttpGet("costs")]
    [Authorize(Policy = Permissions.UsageRead)]
    public async Task<ActionResult<OpenAiPage<OpenAiCostResult>>> GetCosts(
        [FromQuery(Name = "start_date")] string? startDateRaw,
        [FromQuery(Name = "end_date")] string? endDateRaw,
        [FromQuery(Name = "project_ids")] string[]? projectIds)
    {
        var validation = ValidateDates(startDateRaw, endDateRaw, out var startDate, out var endDate);
        if (validation is not null)
        {
            return validation;
        }

        var effectiveProjectIds = await ResolveEffectiveProjectIdsAsync(projectIds);

        try
        {
            var result = await _service.GetCostsAsync(startDate!.Value, endDate, effectiveProjectIds);
            return Ok(result);
        }
        catch (OpenAiException ex)
        {
            return StatusCode(ex.StatusCode, new { message = ex.Message });
        }
    }

    [HttpGet("completions")]
    [Authorize(Policy = Permissions.UsageRead)]
    public async Task<ActionResult<OpenAiPage<OpenAiCompletionsUsageResult>>> GetCompletionsUsage(
        [FromQuery(Name = "start_date")] string? startDateRaw,
        [FromQuery(Name = "end_date")] string? endDateRaw,
        [FromQuery(Name = "project_ids")] string[]? projectIds)
    {
        var validation = ValidateDates(startDateRaw, endDateRaw, out var startDate, out var endDate);
        if (validation is not null)
        {
            return validation;
        }

        var effectiveProjectIds = await ResolveEffectiveProjectIdsAsync(projectIds);

        try
        {
            var result = await _service.GetCompletionsUsageAsync(startDate!.Value, endDate, effectiveProjectIds);
            return Ok(result);
        }
        catch (OpenAiException ex)
        {
            return StatusCode(ex.StatusCode, new { message = ex.Message });
        }
    }

    // Restricción por cliente: si el usuario tiene un cliente asociado (claim 'client_id'),
    // solo puede consultar el proyecto de OpenAI de ese cliente, sin importar los project_ids
    // que envíe el frontend. Si el cliente no tiene project_id configurado, se consulta sin
    // filtro (sin enviar project_ids a OpenAI), según lo acordado con el negocio.
    private async Task<IReadOnlyList<string>?> ResolveEffectiveProjectIdsAsync(string[]? requested)
    {
        var clientId = GetClientId();
        if (clientId is null)
        {
            return Sanitize(requested);
        }

        var client = await _clientService.GetByIdAsync(clientId.Value);
        var projectId = client?.ProjectId?.Trim();
        return string.IsNullOrEmpty(projectId) ? null : [projectId];
    }

    // Descarta ids vacíos/duplicados; si no queda ninguno devuelve null (= sin filtro).
    private static IReadOnlyList<string>? Sanitize(string[]? projectIds)
    {
        var cleaned = projectIds?
            .Select(id => id?.Trim())
            .Where(id => !string.IsNullOrEmpty(id))
            .Select(id => id!)
            .Distinct()
            .ToArray();

        return cleaned is { Length: > 0 } ? cleaned : null;
    }

    private int? GetClientId()
    {
        var raw = User.FindFirstValue(Permissions.ClientClaimType);
        return int.TryParse(raw, out var clientId) ? clientId : null;
    }

    // La API de OpenAI exige start_time, por lo que start_date es obligatorio en estos endpoints.
    private BadRequestObjectResult? ValidateDates(
        string? startDateRaw, string? endDateRaw, out DateTime? startDate, out DateTime? endDate)
    {
        startDate = null;
        endDate = null;

        if (!DateQueryParser.TryParse(startDateRaw, out startDate)
            || !DateQueryParser.TryParse(endDateRaw, out endDate))
        {
            return BadRequest($"Formato de fecha inválido. Use '{DateQueryParser.Format}' (ej: 20260131).");
        }

        if (!startDate.HasValue)
        {
            return BadRequest("El parámetro 'start_date' es obligatorio.");
        }

        if (startDate.HasValue && endDate.HasValue && startDate > endDate)
        {
            return BadRequest("El parámetro 'start_date' no puede ser posterior a 'end_date'.");
        }

        return null;
    }
}
