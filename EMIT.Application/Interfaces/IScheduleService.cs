using EMIT.Application.DTOs;
using EMIT.Domain.Enums;

namespace EMIT.Application.Interfaces;

public interface IScheduleService
{
    Task<IEnumerable<ScheduleDto>> GetAllAsync();
    Task<IEnumerable<ScheduleDto>> GetByLevelAsync(StudentLevel? level);
    Task<ScheduleDto?> GetByIdAsync(int id);
    Task<ScheduleDto> CreateAsync(CreateScheduleDto dto);
    Task<ScheduleDto> UpdateAsync(UpdateScheduleDto dto);
    Task DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<IEnumerable<ScheduleDto>> GetTimetableAsync(StudentLevel? level);
    Task<IEnumerable<ScheduleDto>> GetTodaySchedulesAsync();
    Task<IEnumerable<ScheduleDto>> GetByTeacherAsync(int teacherId);
}
