using System.ComponentModel.DataAnnotations;

namespace Veimen_API.Models.Dtos;

public class UpdateUserRequest
{
    [EmailAddress]
    [MaxLength(150)]
    public string? Email { get; set; }

    [MaxLength(150)]
    public string? FullName { get; set; }

    public long? ProfileId { get; set; }

    public bool? Active { get; set; }
}
