using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Veimen_API.Models;

[Table("prompt")]
public class Prompt
{
    [Column("prompt_id")]
    [Key]
    public long PromptId { get; set; }

    [Column("secuence")]
    public int? Secuence { get; set; }

    [Column("code")]
    [Required]
    [MaxLength(80)]
    public string Code { get; set; } = string.Empty;

    [Column("name")]
    [Required]
    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [Column("description")]
    [MaxLength(500)]
    public string? Description { get; set; }

    [Column("agent")]
    [MaxLength(100)]
    public string? Agent { get; set; }

    [Column("agent_group")]
    [Required]
    [MaxLength(200)]
    public string AgentGroup { get; set; } = string.Empty;

    [Column("type")]
    [MaxLength(50)]
    public string? Type { get; set; }

    [Column("llm_model")]
    [MaxLength(100)]
    public string? LlmModel { get; set; }

    [Column("version")]
    [Required]
    [MaxLength(20)]
    public string Version { get; set; } = "1.0";

    [Column("system_prompt")]
    [Required]
    public string SystemPrompt { get; set; } = string.Empty;

    [Column("user_prompt")]
    public string? UserPrompt { get; set; }

    [Column("temperature", TypeName = "decimal(3, 2)")]
    public decimal? Temperature { get; set; } = 0.20m;

    [Column("max_tokens")]
    public int? MaxTokens { get; set; }

    [Column("active")]
    public bool? Active { get; set; } = true;

    [Column("observations")]
    public string? Observations { get; set; }

    [Column("created_by")]
    [MaxLength(100)]
    public string? CreatedBy { get; set; }

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }

    [Column("updated_by")]
    [MaxLength(100)]
    public string? UpdatedBy { get; set; }

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    [Column("schema_output")]
    public string? SchemaOutput { get; set; }
}
