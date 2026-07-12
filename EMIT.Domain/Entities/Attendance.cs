using System.ComponentModel.DataAnnotations;

namespace EMIT.Domain.Entities;

public class Attendance : BaseEntity
{
    [Required]
    public int ScheduleId { get; set; }

    public Schedule Schedule { get; set; } = null!;

    [Required]
    public int StudentId { get; set; }

    public Student Student { get; set; } = null!;

    public bool IsPresent { get; set; }

    public DateTime Date { get; set; } = DateTime.UtcNow;

    [StringLength(500)]
    public string? Notes { get; set; }
}
