using System.ComponentModel.DataAnnotations;

namespace EMIT.Application.DTOs;

public class NotificationDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public string? TargetRole { get; set; }
    public string? TargetLevel { get; set; }
    public string? CreatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateNotificationDto
{
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(500)]
    public string Message { get; set; } = string.Empty;

    [Required]
    public string Type { get; set; } = string.Empty;

    public string? TargetRole { get; set; }

    public string? TargetLevel { get; set; }

    public string? CreatedByUserId { get; set; }
}
