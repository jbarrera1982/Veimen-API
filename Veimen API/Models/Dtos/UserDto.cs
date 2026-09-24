namespace Veimen_API.Models.Dtos;

public class UserDto
{
    public long UserId { get; set; }

    public string Username { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? FullName { get; set; }

    public bool Active { get; set; }

    public DateTime? LastLoginAt { get; set; }
}