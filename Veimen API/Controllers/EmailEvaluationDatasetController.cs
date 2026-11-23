using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Veimen_API.Exceptions;
using Veimen_API.Models;
using Veimen_API.Models.Dtos;
using Veimen_API.Services;

namespace Veimen_API.Controllers;

// CRUD del dataset de correos de evaluación. La PK es `id` (autonumérica). Cada caso
// pertenece a un cliente. Un usuario con claim
// `client_id` solo ve y modifica los casos de su cliente.
[ApiController]
[Route("api/email-evaluation-dataset")]
[Authorize]
public class EmailEvaluationDatasetController : ControllerBase
{
    private const int MaxPageSize = 100;

    private readonly IEmailEvaluationDatasetService _service;
    private readonly IEmailEvaluationService _evaluationService;

    public EmailEvaluationDatasetController(
        IEmailEvaluationDatasetService service,
        IEmailEvaluationService evaluationService)
    {
        _service = service;
        _evaluationService = evaluationService;
    }

    [HttpGet]
    [Authorize(Policy = Permissions.EmailEvaluationRead)]
    public async Task<ActionResult<PagedResult<EmailEvaluationCaseListItem>>> GetAll(
        [FromQuery] EmailEvaluationQueryParams query)
    {
        if (query.Page < 1)
        {
            return BadRequest("El parámetro 'page' debe ser mayor o igual a 1.");
        }

        if (query.PageSize < 1 || query.PageSize > MaxPageSize)
        {
            return BadRequest($"El parámetro 'pageSize' debe estar entre 1 y {MaxPageSize}.");
        }

        var clientId = GetAssociatedClientId() ?? query.ClientId;
        var result = await _service.GetPagedAsync(
            clientId, query.Scenario, query.Search, query.Page, query.PageSize);

        return Ok(result);
    }

    [HttpGet("scenarios")]
    [Authorize(Policy = Permissions.EmailEvaluationRead)]
    public async Task<ActionResult<IEnumerable<string>>> GetScenarios(
        [FromQuery(Name = "client_id")] int? clientId)
    {
        var effectiveClientId = GetAssociatedClientId() ?? clientId;
        var scenarios = await _service.GetScenariosAsync(effectiveClientId);
        return Ok(scenarios);
    }

    [HttpGet("{id:long}")]
    [Authorize(Policy = Permissions.EmailEvaluationRead)]
    public async Task<ActionResult<EmailEvaluationDataset>> GetById(long id)
    {
        var entity = await _service.GetByIdAsync(id);
        if (entity is null || !CanAccess(entity))
        {
            return NotFound();
        }

        return Ok(entity);
    }

    [HttpPost]
    [Authorize(Policy = Permissions.EmailEvaluationWrite)]
    public async Task<ActionResult<EmailEvaluationDataset>> Create(
        [FromBody] EmailEvaluationDataset entity)
    {
        // El id lo asigna la base: cualquier valor que llegue en el cuerpo se ignora.
        entity.Id = 0;

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (!TryResolveClientId(entity))
        {
            return BadRequest(ModelState);
        }

        var id = await _service.CreateAsync(entity, GetUserEmail());
        var created = await _service.GetByIdAsync(id);

        return CreatedAtAction(nameof(GetById), new { id }, created);
    }

    [HttpPut("{id:long}")]
    [Authorize(Policy = Permissions.EmailEvaluationWrite)]
    public async Task<IActionResult> Update(long id, [FromBody] EmailEvaluationDataset entity)
    {
        // El id de la ruta es el que manda; el del cuerpo no se usa.
        entity.Id = id;

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var existing = await _service.GetByIdAsync(id);
        if (existing is null || !CanAccess(existing))
        {
            return NotFound();
        }

        if (!TryResolveClientId(entity))
        {
            return BadRequest(ModelState);
        }

        var updated = await _service.UpdateAsync(entity, GetUserEmail());
        if (!updated)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpDelete("{id:long}")]
    [Authorize(Policy = Permissions.EmailEvaluationWrite)]
    public async Task<IActionResult> Delete(long id)
    {
        var existing = await _service.GetByIdAsync(id);
        if (existing is null || !CanAccess(existing))
        {
            return NotFound();
        }

        var deleted = await _service.DeleteAsync(id, GetUserEmail());
        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }

    // Envía el caso al webhook de n8n con el mismo cuerpo que recibe un correo
    // real (message_id, receipt_date, from, subject, body, body_html) y devuelve
    // la respuesta del flujo. No se guarda historial de ejecuciones.
    [HttpPost("{id:long}/evaluate")]
    [Authorize(Policy = Permissions.EmailEvaluationWrite)]
    public async Task<IActionResult> Evaluate(long id)
    {
        var entity = await _service.GetByIdAsync(id);
        if (entity is null || !CanAccess(entity))
        {
            return NotFound();
        }

        try
        {
            var result = await _evaluationService.EvaluateAsync(entity);

            if (string.IsNullOrWhiteSpace(result))
            {
                return NoContent();
            }

            try
            {
                using var document = JsonDocument.Parse(result);
                return Ok(document.RootElement.Clone());
            }
            catch (JsonException)
            {
                return Content(result, "text/plain");
            }
        }
        catch (WebhookException ex)
        {
            return StatusCode(ex.StatusCode, new { message = ex.Message });
        }
    }

    // Usuario con cliente asociado: el caso se guarda con su cliente. Sin cliente
    // asociado, el client_id del body es obligatorio.
    private bool TryResolveClientId(EmailEvaluationDataset entity)
    {
        var clientId = GetAssociatedClientId() ?? entity.ClientId;
        if (clientId <= 0)
        {
            ModelState.AddModelError(
                nameof(EmailEvaluationDataset.ClientId), "El cliente es obligatorio.");
            return false;
        }

        entity.ClientId = clientId;
        return true;
    }

    // Usuario con cliente asociado: solo ve y modifica sus casos.
    private bool CanAccess(EmailEvaluationDataset entity)
    {
        var clientId = GetAssociatedClientId();
        return clientId is null || entity.ClientId == clientId;
    }

    private int? GetAssociatedClientId()
    {
        var raw = User.FindFirstValue(Permissions.ClientClaimType);
        return int.TryParse(raw, out var clientId) ? clientId : null;
    }

    private string? GetUserEmail()
    {
        return User.FindFirstValue(JwtRegisteredClaimNames.Email)
            ?? User.FindFirstValue(ClaimTypes.Email);
    }
}
