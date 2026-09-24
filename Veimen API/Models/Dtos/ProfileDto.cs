namespace Veimen_API.Models.Dtos;

public class ProfileDto
{
    public long ProfileId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }
}
