using Veimen_API.Exceptions;
using Veimen_API.Helpers;
using Veimen_API.Models.Dtos;
using Veimen_API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Veimen_API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsageController : ControllerBase
{
    private readonly IUsageService _service;

    public UsageController(IUsageService service)
    {
        _service = service;
    }

    [HttpGet("costs")]
    [Authorize(Policy = Permissions.UsageRead)]
    public async Task<ActionResult<OpenAiPage<OpenAiCostResult>>> GetCosts(
        [FromQuery(Name = "start_date")] string? startDateRaw,
        [FromQuery(Name = "end_date")] string? endDateRaw)
    {
        var validation = ValidateDates(startDateRaw, endDateRaw, out var startDate, out var endDate);
        if (validation is not null)
        {
            return validation;
        }

        try
        {
            var result = await _service.GetCostsAsync(startDate!.Value, endDate);
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
        [FromQuery(Name = "end_date")] string? endDateRaw)
    {
        var validation = ValidateDates(startDateRaw, endDateRaw, out var startDate, out var endDate);
        if (validation is not null)
        {
            return validation;
        }

        try
        {
            var result = await _service.GetCompletionsUsageAsync(startDate!.Value, endDate);
            return Ok(result);
        }
        catch (OpenAiException ex)
        {
            return StatusCode(ex.StatusCode, new { message = ex.Message });
        }
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
