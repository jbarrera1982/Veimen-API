using Veimen_API.Models;
using Veimen_API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Veimen_API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PromptsController : ControllerBase
{
    private readonly IPromptService _service;

    public PromptsController(IPromptService service)
    {
        _service = service;
    }

    [HttpGet]
    [Authorize(Policy = Permissions.PromptsRead)]
    public async Task<ActionResult<IEnumerable<Prompt>>> GetAll([FromQuery(Name = "client_id")] int? clientId)
    {
        var prompts = await _service.GetAllAsync(GetAssociatedClientId() ?? clientId);
        return Ok(prompts);
    }

    [HttpGet("{id:long}")]
    [Authorize(Policy = Permissions.PromptsRead)]
    public async Task<ActionResult<Prompt>> GetById(long id)
    {
        var prompt = await _service.GetByIdAsync(id);
        if (prompt is null || !CanAccess(prompt))
        {
            return NotFound();
        }

        return Ok(prompt);
    }

    [HttpPost]
    [Authorize(Policy = Permissions.PromptsWrite)]
    public async Task<ActionResult<Prompt>> Create([FromBody] Prompt prompt)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        prompt.ClientId = GetAssociatedClientId() ?? prompt.ClientId;

        var newId = await _service.CreateAsync(prompt);
        var createdPrompt = await _service.GetByIdAsync(newId);

        return CreatedAtAction(nameof(GetById), new { id = newId }, createdPrompt);
    }

    [HttpPut("{id:long}")]
    [Authorize(Policy = Permissions.PromptsWrite)]
    public async Task<IActionResult> Update(long id, [FromBody] Prompt prompt)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (id != prompt.PromptId)
        {
            return BadRequest("El id de la ruta no coincide con el id del cuerpo.");
        }

        var existing = await _service.GetByIdAsync(id);
        if (existing is null || !CanAccess(existing))
        {
            return NotFound();
        }

        prompt.ClientId = GetAssociatedClientId() ?? prompt.ClientId;

        var updated = await _service.UpdateAsync(id, prompt);
        if (!updated)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpDelete("{id:long}")]
    [Authorize(Policy = Permissions.PromptsWrite)]
    public async Task<IActionResult> Delete(long id)
    {
        var existing = await _service.GetByIdAsync(id);
        if (existing is null || !CanAccess(existing))
        {
            return NotFound();
        }

        var deleted = await _service.DeleteAsync(id);
        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }

    // Usuario con cliente asociado (claim 'client_id'): solo ve y modifica sus prompts.
    private bool CanAccess(Prompt prompt)
    {
        var clientId = GetAssociatedClientId();
        return clientId is null || prompt.ClientId == clientId;
    }

    private int? GetAssociatedClientId()
    {
        var raw = User.FindFirstValue(Permissions.ClientClaimType);
        return int.TryParse(raw, out var clientId) ? clientId : null;
    }
}
