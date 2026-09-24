using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Veimen_API.Models;

[Table("service_request")]
public class ServiceRequest
{
    // Valores válidos del enum 'status' en MySQL (respetar mayúsculas exactas, ej: 'aNalyzed').
    public static readonly string[] ValidStatuses =
        ["Received", "Rejected", "Closed", "Awaiting OTP", "Interrupted", "aNalyzed"];

    [Column("request_number")]
    [Key]
    public long RequestNumber { get; set; }

    [Column("channel")]
    [Required]
    [MaxLength(30)]
    public string Channel { get; set; } = string.Empty;

    [Column("from")]
    [Required]
    [MaxLength(250)]
    public string From { get; set; } = string.Empty;

    [Column("subject")]
    [MaxLength(500)]
    public string? Subject { get; set; }

    [Column("original_message")]
    public string? OriginalMessage { get; set; }

    [Column("receipt_date")]
    public DateTime ReceiptDate { get; set; }

    [Column("n8n_workflow")]
    [Required]
    [MaxLength(200)]
    public string N8nWorkflow { get; set; } = string.Empty;

    [Column("status")]
    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "Received";

    [Column("priority")]
    [Required]
    [MaxLength(10)]
    public string Priority { get; set; } = "Normal";

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    [Column("description")]
    public string? Description { get; set; }

    [Column("otp_code")]
    [MaxLength(10)]
    public string? OtpCode { get; set; }

    [Column("otp_verified_at")]
    public DateTime? OtpVerifiedAt { get; set; }

    [Column("detected_intent")]
    [MaxLength(100)]
    public string? DetectedIntent { get; set; }

    [Column("audited")]
    public bool Audited { get; set; }

    [Column("suspension_type")]
    [MaxLength(20)]
    public string? SuspensionType { get; set; }
}
