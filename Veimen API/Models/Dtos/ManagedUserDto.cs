namespace Veimen_API.Models.Dtos;

public class ManagedUserDto
{
    public long UserId { get; set; }

    public string Username { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? FullName { get; set; }

    public bool Active { get; set; }

    public long? ProfileId { get; set; }

    public string? ProfileName { get; set; }

    public DateTime? LastLoginAt { get; set; }

    public DateTime CreatedAt { get; set; }
}
