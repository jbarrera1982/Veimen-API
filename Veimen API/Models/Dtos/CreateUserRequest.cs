using System.ComponentModel.DataAnnotations;

namespace Veimen_API.Models.Dtos;

public class CreateUserRequest
{
    [Required]
    [MaxLength(50)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(150)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(8)]
    [MaxLength(100)]
    public string Password { get; set; } = string.Empty;

    [MaxLength(150)]
    public string? FullName { get; set; }

    [Required]
    public long ProfileId { get; set; }
}
