using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
        var result = await _mailService.ScanAsync(request);
        return Ok(result);
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
    public async Task<ActionResult<IEnumerable<MailComparisonDto>>> GetComparison([FromQuery] string? start_date, [FromQuery] string? end_date)
    {
        var result = await _mailService.GetComparisonAsync(start_date, end_date);
        return Ok(result);
    }
}
