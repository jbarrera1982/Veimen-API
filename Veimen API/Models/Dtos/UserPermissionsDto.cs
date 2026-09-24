namespace Veimen_API.Models.Dtos;

public class UserPermissionsDto
{
    public string? Profile { get; set; }

    public string[] Permissions { get; set; } = [];
}
