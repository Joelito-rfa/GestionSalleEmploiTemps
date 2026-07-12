using EMIT.Domain.Enums;

namespace GestionSalleEmploiTemps.Models;

public class LevelTimetableViewModel
{
    public StudentLevel Level { get; set; }
    public List<DayScheduleViewModel> Days { get; set; } = new();
}

public class DayScheduleViewModel
{
    public DayOfWeek Day { get; set; }
    public List<ScheduleViewModel> Schedules { get; set; } = new();
}

public class ScheduleViewModel
{
    public int Id { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string RoomName { get; set; } = string.Empty;
    public string TeacherName { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
}
