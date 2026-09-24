using System.ComponentModel.DataAnnotations;

namespace Veimen_API.Models.Dtos;

public class RefreshTokenRequest
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}