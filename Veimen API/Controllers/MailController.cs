using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Veimen_API.Exceptions;
using Veimen_API.Models.Dtos;
using Veimen_API.Services;

namespace Veimen_API.Controllers;

[ApiController]
[Route("api/mail")]
[Authorize]
public class MailController : ControllerBase
{
    private readonly IMailService _mailService;

    public MailController(IMailService mailService)
    {
        _mailService = mailService;
    }

    [HttpPost("scan")]
    [Authorize(Policy = Permissions.MailScan)]
    public async Task<ActionResult<IEnumerable<MailDailySummaryDto>>> Scan([FromBody] MailScanRequest? request)
    {
        request ??= new MailScanRequest();
        try
        {
            var result = await _mailService.ScanAsync(request);
            return Ok(result);
        }
        catch (MailException ex)
        {
            return StatusCode(ex.StatusCode, new { message = ex.Message });
        }
    }

    [HttpGet("summary")]
    [Authorize(Policy = Permissions.MailScan)]
    public async Task<ActionResult<IEnumerable<MailDailySummaryDto>>> GetSummary([FromQuery] string? start_date, [FromQuery] string? end_date)
    {
        var result = await _mailService.GetSummaryAsync(start_date, end_date);
        return Ok(result);
    }

    [HttpGet("comparison")]
    [Authorize(Policy = Permissions.MailScan)]
    public async Task<ActionResult<IEnumerable<MailComparisonDto>>> GetComparison(
        [FromQuery] string? start_date,
        [FromQuery] string? end_date,
        [FromQuery(Name = "client_id")] int? clientId)
    {
        // Escanear el buzón antes de comparar para que los datos estén actualizados.
        // Escaneo incremental (sin force): solo procesa los correos nuevos desde el último escaneo.
        try
        {
            await _mailService.ScanAsync(new MailScanRequest());

            // Filtro por cliente: el correo se atribuye a un cliente en mail_message.client_id
            // y el requerimiento en service_request.client_id. Ausente = "Todos".
            // Un usuario con cliente asociado (claim 'client_id') solo puede consultar el suyo,
            // sin importar el client_id que envíe el frontend.
            var result = await _mailService.GetComparisonAsync(
                start_date, end_date, GetAssociatedClientId() ?? clientId);
            return Ok(result);
        }
        catch (MailException ex)
        {
            return StatusCode(ex.StatusCode, new { message = ex.Message });
        }
    }

    [HttpGet("pending-count")]
    [Authorize(Policy = Permissions.MailScan)]
    public async Task<ActionResult<MailPendingCountDto>> GetPendingCount(
        [FromQuery(Name = "client_id")] int? clientId)
    {
        // Mismo criterio que GetComparison: un usuario con cliente asociado
        // (claim 'client_id') solo puede consultar el suyo.
        try
        {
            var result = await _mailService.GetPendingCountAsync(
                GetAssociatedClientId() ?? clientId);
            return Ok(result);
        }
        catch (MailException ex)
        {
            return StatusCode(ex.StatusCode, new { message = ex.Message });
        }
    }

    // Cliente asociado al usuario autenticado (claim 'client_id'), null si no tiene.
    private int? GetAssociatedClientId()
    {
        var raw = User.FindFirstValue(Permissions.ClientClaimType);
        return int.TryParse(raw, out var clientId) ? clientId : null;
    }
}
