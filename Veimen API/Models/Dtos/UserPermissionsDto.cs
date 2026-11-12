namespace Veimen_API.Models.Dtos;

public class UserPermissionsDto
{
    public string? Profile { get; set; }

    // Idioma preferido leído del claim 'language' del access token.
    public string? Language { get; set; }

    public string[] Permissions { get; set; } = [];
}
