using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Veimen_API.Models;

[Table("user")]
public class User
{
    [Column("user_id")]
    [Key]
    public long UserId { get; set; }

    [Column("username")]
    [Required]
    [MaxLength(50)]
    public string Username { get; set; } = string.Empty;

    [Column("email")]
    [Required]
    [MaxLength(150)]
    public string Email { get; set; } = string.Empty;

    [Column("password_hash")]
    [Required]
    [MaxLength(100)]
    public string PasswordHash { get; set; } = string.Empty;

    [Column("full_name")]
    [MaxLength(150)]
    public string? FullName { get; set; }

    [Column("active")]
    public bool Active { get; set; } = true;

    [Column("profile_id")]
    public long? ProfileId { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }

    [Column("last_login_at")]
    public DateTime? LastLoginAt { get; set; }
}