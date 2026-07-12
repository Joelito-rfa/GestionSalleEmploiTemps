using System.ComponentModel.DataAnnotations;

namespace EMIT.Domain.Entities;

public class Notification : BaseEntity
{
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(500)]
    public string Message { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Type { get; set; } = string.Empty;

    public bool IsRead { get; set; }

    [StringLength(50)]
    public string? TargetRole { get; set; }

    [StringLength(10)]
    public string? TargetLevel { get; set; }

    public string? CreatedByUserId { get; set; }
}
