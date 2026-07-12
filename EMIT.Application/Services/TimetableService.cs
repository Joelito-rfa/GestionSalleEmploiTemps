using EMIT.Application.DTOs;
using EMIT.Application.Interfaces;
using EMIT.Domain.Entities;
using EMIT.Domain.Enums;
using EMIT.Domain.Interfaces;

namespace EMIT.Application.Services;

public class TimetableService : ITimetableService
{
    private readonly IUnitOfWork _unitOfWork;

    public TimetableService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<ScheduleDto>> GetTimetableAsync(StudentLevel? level)
    {
        var schedules = await _unitOfWork.Repository<Schedule>().GetAllIncludingAsync(s => s.Room, s => s.Teacher);

        if (level.HasValue)
            schedules = schedules.Where(s => s.Level == level.Value).ToList();

        return schedules.OrderBy(s => s.Day).ThenBy(s => s.StartTime).Select(s => new ScheduleDto
        {
            Id = s.Id,
            Day = s.Day,
            StartTime = s.StartTime,
            EndTime = s.EndTime,
            Level = s.Level,
            RoomId = s.RoomId,
            RoomName = s.Room?.Name ?? "N/A",
            TeacherId = s.TeacherId,
            TeacherName = s.Teacher?.FullName ?? "N/A",
            Subject = s.Teacher?.Subject ?? "N/A"
        });
    }

    public async Task<IEnumerable<ScheduleDto>> SearchAsync(string? searchTerm, StudentLevel? level, DayOfWeek? day)
    {
        var schedules = await _unitOfWork.Repository<Schedule>().GetAllIncludingAsync(s => s.Room, s => s.Teacher);

        if (level.HasValue)
            schedules = schedules.Where(s => s.Level == level.Value).ToList();

        if (day.HasValue)
            schedules = schedules.Where(s => s.Day == day.Value).ToList();

        var query = schedules.OrderBy(s => s.Day).ThenBy(s => s.StartTime).AsEnumerable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.ToLower();
            query = query.Where(s =>
                s.Room?.Name.ToLower().Contains(term) == true ||
                s.Teacher?.FullName.ToLower().Contains(term) == true ||
                s.Teacher?.Subject.ToLower().Contains(term) == true);
        }

        return query.Select(s => new ScheduleDto
        {
            Id = s.Id,
            Day = s.Day,
            StartTime = s.StartTime,
            EndTime = s.EndTime,
            Level = s.Level,
            RoomId = s.RoomId,
            RoomName = s.Room?.Name ?? "N/A",
            TeacherId = s.TeacherId,
            TeacherName = s.Teacher?.FullName ?? "N/A",
            Subject = s.Teacher?.Subject ?? "N/A"
        });
    }
}
