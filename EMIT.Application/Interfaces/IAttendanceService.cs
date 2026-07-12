using EMIT.Application.DTOs;

namespace EMIT.Application.Interfaces;

public interface IAttendanceService
{
    Task<IEnumerable<AttendanceDto>> GetByScheduleAsync(int scheduleId);
    Task<IEnumerable<AttendanceDto>> GetByStudentAsync(int studentId);
    Task<IEnumerable<AttendanceDto>> GetTodayAllAsync();
    Task<AttendanceDto> MarkAsync(MarkAttendanceDto dto);
    Task MarkBulkAsync(BulkAttendanceDto dto);
    Task<bool> HasAttendanceForDateAsync(int scheduleId, DateTime date);
}
