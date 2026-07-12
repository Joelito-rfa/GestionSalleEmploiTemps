using System.ComponentModel.DataAnnotations;

namespace EMIT.Application.DTOs;

public class AttendanceDto
{
    public int Id { get; set; }
    public int ScheduleId { get; set; }
    public string? ScheduleInfo { get; set; }
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public bool IsPresent { get; set; }
    public DateTime Date { get; set; }
    public string? Notes { get; set; }
}

public class MarkAttendanceDto
{
    [Required]
    public int ScheduleId { get; set; }

    [Required]
    public int StudentId { get; set; }

    public bool IsPresent { get; set; }

    public string? Notes { get; set; }
}

public class BulkAttendanceDto
{
    [Required]
    public int ScheduleId { get; set; }

    public DateTime Date { get; set; } = DateTime.UtcNow;

    public List<StudentAttendanceItem> Students { get; set; } = new();
}

public class StudentAttendanceItem
{
    public int StudentId { get; set; }
    public bool IsPresent { get; set; }
    public string? Notes { get; set; }
}
