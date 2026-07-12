using EMIT.Application.DTOs;
using EMIT.Domain.Enums;

namespace EMIT.Application.Interfaces;

public interface ITimetableService
{
    Task<IEnumerable<ScheduleDto>> GetTimetableAsync(StudentLevel? level);
    Task<IEnumerable<ScheduleDto>> SearchAsync(string? searchTerm, StudentLevel? level, DayOfWeek? day);
}
