using System.ComponentModel.DataAnnotations;

namespace Veimen_API.Models.Dtos;

public class ResetPasswordRequest
{
    [Required]
    [MinLength(8)]
    [MaxLength(100)]
    public string NewPassword { get; set; } = string.Empty;
}
