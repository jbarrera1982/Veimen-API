using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Veimen_API.Models;

[Table("client")]
public class Client
{
    [Column("client_id")]
    [Key]
    public int ClientId { get; set; }

    [Column("name")]
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Column("inbound_email")]
    [MaxLength(100)]
    [EmailAddress]
    public string? InboundEmail { get; set; }

    [Column("outbound_email")]
    [MaxLength(100)]
    [EmailAddress]
    public string? OutboundEmail { get; set; }

    [Column("analyst_email")]
    [MaxLength(100)]
    [EmailAddress]
    public string? AnalystEmail { get; set; }

    [Column("openAI_api_key")]
    [MaxLength(100)]
    public string? OpenAIApiKey { get; set; }

    [Column("active")]
    public bool Active { get; set; } = true;
}
