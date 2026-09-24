using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Veimen_API.Models;

[Table("refresh_token")]
public class RefreshToken
{
    [Column("refresh_token_id")]
    [Key]
    public long RefreshTokenId { get; set; }

    [Column("user_id")]
    public long UserId { get; set; }

    [Column("token_sha256")]
    [Required]
    [MaxLength(64)]
    public string TokenSha256 { get; set; } = string.Empty;

    [Column("expires_at")]
    public DateTime ExpiresAt { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("revoked_at")]
    public DateTime? RevokedAt { get; set; }

    [Column("replaced_by_token")]
    [MaxLength(64)]
    public string? ReplacedByToken { get; set; }

    [Column("ip_address")]
    [MaxLength(45)]
    public string? IpAddress { get; set; }

    [Column("user_agent")]
    [MaxLength(255)]
    public string? UserAgent { get; set; }
}