using EMIT.Application.DTOs;
using EMIT.Application.Interfaces;
using EMIT.Domain.Entities;
using EMIT.Domain.Enums;
using EMIT.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace EMIT.Application.Services;

public class ScheduleService : IScheduleService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ScheduleService> _logger;

    public ScheduleService(IUnitOfWork unitOfWork, ILogger<ScheduleService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<IEnumerable<ScheduleDto>> GetAllAsync()
    {
        var schedules = await _unitOfWork.Repository<Schedule>()
            .GetAllIncludingAsync(s => s.Room, s => s.Teacher);
        return schedules.Select(MapToDto).OrderBy(s => s.Day).ThenBy(s => s.StartTime);
    }

    public async Task<IEnumerable<ScheduleDto>> GetByLevelAsync(StudentLevel? level)
    {
        if (level.HasValue)
        {
            var filtered = await _unitOfWork.Repository<Schedule>()
                .FindIncludingAsync(s => s.Level == level.Value, s => s.Room, s => s.Teacher);
            return filtered.Select(MapToDto).OrderBy(s => s.Day).ThenBy(s => s.StartTime);
        }
        return await GetAllAsync();
    }

    public async Task<ScheduleDto?> GetByIdAsync(int id)
    {
        var schedules = await _unitOfWork.Repository<Schedule>()
            .FindIncludingAsync(s => s.Id == id, s => s.Room, s => s.Teacher);
        return schedules.Select(MapToDto).FirstOrDefault();
    }

    public async Task<ScheduleDto> CreateAsync(CreateScheduleDto dto)
    {
        var schedule = new Schedule
        {
            Day = dto.Day,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
            Level = dto.Level,
            RoomId = dto.RoomId,
            TeacherId = dto.TeacherId
        };

        await ValidateNoConflictAsync(schedule);

        await _unitOfWork.Repository<Schedule>().AddAsync(schedule);
        await _unitOfWork.SaveChangesAsync();
        _logger.LogInformation("Schedule created: {Level} {Day} {Start}-{End}", schedule.Level, schedule.Day, schedule.StartTime, schedule.EndTime);

        var created = await _unitOfWork.Repository<Schedule>()
            .FindIncludingAsync(s => s.Id == schedule.Id, s => s.Room, s => s.Teacher);
        return MapToDto(created.First()!);
    }

    public async Task<ScheduleDto> UpdateAsync(UpdateScheduleDto dto)
    {
        var repo = _unitOfWork.Repository<Schedule>();
        var schedule = await repo.GetByIdAsync(dto.Id) ?? throw new KeyNotFoundException($"Séance avec l'ID {dto.Id} introuvable.");
        schedule.Day = dto.Day;
        schedule.StartTime = dto.StartTime;
        schedule.EndTime = dto.EndTime;
        schedule.Level = dto.Level;
        schedule.RoomId = dto.RoomId;
        schedule.TeacherId = dto.TeacherId;

        await ValidateNoConflictAsync(schedule);

        repo.Update(schedule);
        await _unitOfWork.SaveChangesAsync();
        _logger.LogInformation("Schedule updated: {Id}", schedule.Id);

        var updated = await _unitOfWork.Repository<Schedule>()
            .FindIncludingAsync(s => s.Id == schedule.Id, s => s.Room, s => s.Teacher);
        return MapToDto(updated.First()!);
    }

    public async Task DeleteAsync(int id)
    {
        var repo = _unitOfWork.Repository<Schedule>();
        var schedule = await repo.GetByIdAsync(id);
        if (schedule != null)
        {
            repo.Remove(schedule);
            await _unitOfWork.SaveChangesAsync();
            _logger.LogInformation("Schedule deleted: {Id}", id);
        }
    }

    public async Task<bool> ExistsAsync(int id) =>
        await _unitOfWork.Repository<Schedule>().AnyAsync(s => s.Id == id);

    public async Task<IEnumerable<ScheduleDto>> GetTimetableAsync(StudentLevel? level)
    {
        return await GetByLevelAsync(level);
    }

    public async Task<IEnumerable<ScheduleDto>> GetTodaySchedulesAsync()
    {
        var today = DateTime.UtcNow.DayOfWeek;
        var schedules = await _unitOfWork.Repository<Schedule>()
            .FindIncludingAsync(s => s.Day == today, s => s.Room, s => s.Teacher);
        return schedules.Select(MapToDto).OrderBy(s => s.StartTime);
    }

    public async Task<IEnumerable<ScheduleDto>> GetByTeacherAsync(int teacherId)
    {
        var schedules = await _unitOfWork.Repository<Schedule>()
            .FindIncludingAsync(s => s.TeacherId == teacherId, s => s.Room, s => s.Teacher);
        return schedules.Select(MapToDto).OrderBy(s => s.Day).ThenBy(s => s.StartTime);
    }

    private async Task ValidateNoConflictAsync(Schedule schedule)
    {
        var repo = _unitOfWork.Repository<Schedule>();

        var roomConflict = await repo.AnyAsync(s =>
            s.RoomId == schedule.RoomId &&
            s.Day == schedule.Day &&
            s.StartTime < schedule.EndTime &&
            s.EndTime > schedule.StartTime &&
            s.Id != schedule.Id);

        if (roomConflict)
            throw new InvalidOperationException("Cette salle est déjà réservée pour ce créneau.");

        var teacherConflict = await repo.AnyAsync(s =>
            s.TeacherId == schedule.TeacherId &&
            s.Day == schedule.Day &&
            s.StartTime < schedule.EndTime &&
            s.EndTime > schedule.StartTime &&
            s.Id != schedule.Id);

        if (teacherConflict)
            throw new InvalidOperationException("Cet enseignant est déjà occupé sur ce créneau.");
    }

    private static ScheduleDto MapToDto(Schedule schedule) => new()
    {
        Id = schedule.Id,
        Day = schedule.Day,
        StartTime = schedule.StartTime,
        EndTime = schedule.EndTime,
        Level = schedule.Level,
        RoomId = schedule.RoomId,
        RoomName = schedule.Room?.Name ?? "N/D",
        TeacherId = schedule.TeacherId,
        TeacherName = schedule.Teacher?.FullName ?? "N/D",
        Subject = schedule.Teacher?.Subject ?? "N/D"
    };
}
