using System.ComponentModel.DataAnnotations;

namespace Veimen_API.Models.Dtos;

public class LoginRequest
{
    [Required]
    [MaxLength(150)]
    public string Identifier { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}