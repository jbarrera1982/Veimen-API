using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Veimen_API.Exceptions;
using Veimen_API.Models.Dtos;
using Veimen_API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Veimen_API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = Permissions.UsersManage)]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ManagedUserDto>>> List()
    {
        return Ok(await _userService.ListAsync());
    }

    [HttpGet("profiles")]
    public async Task<ActionResult<IReadOnlyList<ProfileDto>>> ListProfiles()
    {
        return Ok(await _userService.ListProfilesAsync());
    }

    [HttpPost]
    public async Task<ActionResult<ManagedUserDto>> Create([FromBody] CreateUserRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var user = await _userService.CreateAsync(request);
            return StatusCode(StatusCodes.Status201Created, user);
        }
        catch (AuthException ex)
        {
            return StatusCode(ex.StatusCode, new { message = ex.Message });
        }
    }

    [HttpPatch("{id:long}")]
    public async Task<ActionResult<ManagedUserDto>> Update(long id, [FromBody] UpdateUserRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var currentUserId = GetUserId();
        if (currentUserId is null)
        {
            return Unauthorized();
        }

        try
        {
            return Ok(await _userService.UpdateAsync(id, request, currentUserId.Value));
        }
        catch (AuthException ex)
        {
            return StatusCode(ex.StatusCode, new { message = ex.Message });
        }
    }

    [HttpPost("{id:long}/reset-password")]
    public async Task<IActionResult> ResetPassword(long id, [FromBody] ResetPasswordRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            await _userService.ResetPasswordAsync(id, request.NewPassword);
            return NoContent();
        }
        catch (AuthException ex)
        {
            return StatusCode(ex.StatusCode, new { message = ex.Message });
        }
    }

    private long? GetUserId()
    {
        var sub = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

        return long.TryParse(sub, out var userId) ? userId : null;
    }
}
