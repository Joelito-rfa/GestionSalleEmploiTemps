using EMIT.Application.DTOs;
using EMIT.Application.Interfaces;
using EMIT.Domain.Entities;
using EMIT.Domain.Interfaces;

namespace EMIT.Application.Services;

public class DashboardService : IDashboardService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IScheduleService _scheduleService;

    public DashboardService(IUnitOfWork unitOfWork, IScheduleService scheduleService)
    {
        _unitOfWork = unitOfWork;
        _scheduleService = scheduleService;
    }

    public async Task<DashboardDto> GetDashboardAsync()
    {
        var studentRepo = _unitOfWork.Repository<Student>();
        var teacherRepo = _unitOfWork.Repository<Teacher>();
        var roomRepo = _unitOfWork.Repository<Room>();
        var scheduleRepo = _unitOfWork.Repository<Schedule>();
        var attendanceRepo = _unitOfWork.Repository<Attendance>();

        var totalStudents = await studentRepo.CountAsync();
        var totalTeachers = await teacherRepo.CountAsync();
        var totalRooms = await roomRepo.CountAsync();
        var totalSchedules = await scheduleRepo.CountAsync();

        var today = DateTime.UtcNow.DayOfWeek;
        var todaySchedules = await scheduleRepo.FindIncludingAsync(s => s.Day == today, s => s.Room, s => s.Teacher);

        var students = await studentRepo.GetAllAsync();
        var studentsByLevel = students
            .GroupBy(s => s.Level.ToString())
            .ToDictionary(g => g.Key, g => g.Count());

        var allSchedules = await scheduleRepo.GetAllAsync();
        var schedulesByDay = allSchedules
            .GroupBy(s => s.Day.ToString())
            .ToDictionary(g => g.Key, g => g.Count());

        var schedulesByLevel = allSchedules
            .GroupBy(s => s.Level.ToString())
            .ToDictionary(g => g.Key, g => g.Count());

        var todayScheduleDtos = todaySchedules.Select(s => new ScheduleDto
        {
            Id = s.Id,
            Day = s.Day,
            StartTime = s.StartTime,
            EndTime = s.EndTime,
            Level = s.Level,
            RoomId = s.RoomId,
            RoomName = s.Room?.Name ?? "N/D",
            TeacherId = s.TeacherId,
            TeacherName = s.Teacher?.FullName ?? "N/D",
            Subject = s.Teacher?.Subject ?? "N/D"
        }).ToList();

        var attendanceToday = await attendanceRepo.CountAsync(a => a.Date.Date == DateTime.UtcNow.Date);

        var weekStart = DateTime.UtcNow.Date.AddDays(-(int)DateTime.UtcNow.DayOfWeek);
        var weekEnd = weekStart.AddDays(7);
        var attendanceWeek = await attendanceRepo.CountAsync(a => a.Date >= weekStart && a.Date < weekEnd);

        var totalPossibleAttendance = todayScheduleDtos.Count * totalStudents;
        var attendanceRate = totalPossibleAttendance > 0
            ? Math.Round((double)attendanceToday / totalPossibleAttendance * 100, 1)
            : 0;

        var roomsWithSchedules = allSchedules.Select(s => s.RoomId).Distinct().Count();
        var roomUtilization = totalRooms > 0
            ? Math.Round((double)roomsWithSchedules / totalRooms * 100, 1)
            : 0;

        var recentSchedules = allSchedules
            .OrderByDescending(s => s.CreatedAt)
            .Take(5)
            .Select(s => new RecentActivityDto
            {
                Description = $"Séance {s.Level} - {GetDayFrenchName(s.Day)} {s.StartTime:hh\\:mm}-{s.EndTime:hh\\:mm}",
                Type = "schedule",
                Date = s.CreatedAt
            }).ToList();

        var recentStudents = (await studentRepo.GetAllAsync())
            .OrderByDescending(s => s.CreatedAt)
            .Take(3)
            .Select(s => new RecentActivityDto
            {
                Description = $"Nouvel étudiant : {s.FullName}",
                Type = "student",
                Date = s.CreatedAt
            }).ToList();

        var recentActivity = recentSchedules.Concat(recentStudents)
            .OrderByDescending(a => a.Date)
            .Take(5)
            .ToList();

        return new DashboardDto
        {
            TotalStudents = totalStudents,
            TotalTeachers = totalTeachers,
            TotalRooms = totalRooms,
            TotalSchedules = totalSchedules,
            TotalSchedulesToday = todayScheduleDtos.Count,
            TotalAttendancesToday = attendanceToday,
            TotalAttendancesWeek = attendanceWeek,
            AttendanceRate = attendanceRate,
            RoomUtilizationPercent = roomUtilization,
            TodaySchedules = todayScheduleDtos.OrderBy(s => s.StartTime),
            StudentsByLevel = studentsByLevel,
            SchedulesByDay = schedulesByDay,
            SchedulesByLevel = schedulesByLevel,
            RecentActivity = recentActivity
        };
    }

    private static string GetDayFrenchName(DayOfWeek day) => day switch
    {
        DayOfWeek.Monday => "Lundi",
        DayOfWeek.Tuesday => "Mardi",
        DayOfWeek.Wednesday => "Mercredi",
        DayOfWeek.Thursday => "Jeudi",
        DayOfWeek.Friday => "Vendredi",
        DayOfWeek.Saturday => "Samedi",
        DayOfWeek.Sunday => "Dimanche",
        _ => day.ToString()
    };
}
