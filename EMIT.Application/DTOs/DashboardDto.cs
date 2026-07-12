namespace EMIT.Application.DTOs;

public class DashboardDto
{
    public int TotalStudents { get; set; }
    public int TotalTeachers { get; set; }
    public int TotalRooms { get; set; }
    public int TotalSchedules { get; set; }
    public int TotalSchedulesToday { get; set; }
    public int TotalAttendancesToday { get; set; }
    public int TotalAttendancesWeek { get; set; }
    public double AttendanceRate { get; set; }
    public double RoomUtilizationPercent { get; set; }
    public IEnumerable<ScheduleDto> TodaySchedules { get; set; } = new List<ScheduleDto>();
    public Dictionary<string, int> StudentsByLevel { get; set; } = new();
    public Dictionary<string, int> SchedulesByDay { get; set; } = new();
    public Dictionary<string, int> SchedulesByLevel { get; set; } = new();
    public List<RecentActivityDto> RecentActivity { get; set; } = new();
}

public class RecentActivityDto
{
    public string Description { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public DateTime Date { get; set; }
}
