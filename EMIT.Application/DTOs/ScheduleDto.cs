using System.ComponentModel.DataAnnotations;
using EMIT.Domain.Enums;

namespace EMIT.Application.DTOs;

public class ScheduleDto
{
    public int Id { get; set; }
    public DayOfWeek Day { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public StudentLevel Level { get; set; }
    public int RoomId { get; set; }
    public string RoomName { get; set; } = string.Empty;
    public int TeacherId { get; set; }
    public string TeacherName { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
}

public class CreateScheduleDto
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

    [Required]
    public int TeacherId { get; set; }
}

public class UpdateScheduleDto
{
    public int Id { get; set; }

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

    [Required]
    public int TeacherId { get; set; }
}
