using Veimen_API.Models;
using Veimen_API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Veimen_API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ClientsController : ControllerBase
{
    private readonly IClientService _service;

    public ClientsController(IClientService service)
    {
        _service = service;
    }

    [HttpGet]
    [Authorize(Policy = Permissions.ClientsRead)]
    public async Task<ActionResult<IEnumerable<Client>>> GetAll()
    {
        var clients = await _service.GetAllAsync();
        return Ok(clients);
    }

    [HttpGet("{id:int}")]
    [Authorize(Policy = Permissions.ClientsRead)]
    public async Task<ActionResult<Client>> GetById(int id)
    {
        var client = await _service.GetByIdAsync(id);
        if (client is null)
        {
            return NotFound();
        }

        return Ok(client);
    }

    [HttpPost]
    [Authorize(Policy = Permissions.ClientsWrite)]
    public async Task<ActionResult<Client>> Create([FromBody] Client client)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var newId = await _service.CreateAsync(client);
        var createdClient = await _service.GetByIdAsync(newId);

        return CreatedAtAction(nameof(GetById), new { id = newId }, createdClient);
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = Permissions.ClientsWrite)]
    public async Task<IActionResult> Update(int id, [FromBody] Client client)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (id != client.ClientId)
        {
            return BadRequest("El id de la ruta no coincide con el id del cuerpo.");
        }

        var updated = await _service.UpdateAsync(id, client);
        if (!updated)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = Permissions.ClientsWrite)]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _service.DeleteAsync(id);
        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}
