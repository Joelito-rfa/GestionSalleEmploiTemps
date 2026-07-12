using System.ComponentModel.DataAnnotations;
using EMIT.Domain.Enums;

namespace EMIT.Domain.Entities;

public class Schedule : BaseEntity
{
    [Required]
    public DayOfWeek Day { get; set; }

    [Required]
    public TimeSpan StartTime { get; set; }

    [Required]
    public TimeSpan EndTime { get; set; }

    [Required]
    public StudentLevel Level { get; set; }

    [Required]
    public int RoomId { get; set; }

    public Room Room { get; set; } = null!;

    [Required]
    public int TeacherId { get; set; }

    public Teacher Teacher { get; set; } = null!;

    public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
}
