using EMIT.Application.DTOs;
using EMIT.Application.Interfaces;
using EMIT.Domain.Entities;
using EMIT.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace EMIT.Application.Services;

public class AttendanceService : IAttendanceService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AttendanceService> _logger;

    public AttendanceService(IUnitOfWork unitOfWork, ILogger<AttendanceService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<IEnumerable<AttendanceDto>> GetByScheduleAsync(int scheduleId)
    {
        var records = await _unitOfWork.Repository<Attendance>().FindAsync(a => a.ScheduleId == scheduleId);
        var dtos = new List<AttendanceDto>();
        foreach (var a in records)
        {
            var student = await _unitOfWork.Repository<Student>().GetByIdAsync(a.StudentId);
            dtos.Add(new AttendanceDto
            {
                Id = a.Id,
                ScheduleId = a.ScheduleId,
                StudentId = a.StudentId,
                StudentName = student?.FullName ?? "N/A",
                IsPresent = a.IsPresent,
                Date = a.Date,
                Notes = a.Notes
            });
        }
        return dtos.OrderBy(a => a.StudentName);
    }

    public async Task<IEnumerable<AttendanceDto>> GetByStudentAsync(int studentId)
    {
        var records = await _unitOfWork.Repository<Attendance>().FindAsync(a => a.StudentId == studentId);
        return records.Select(a => new AttendanceDto
        {
            Id = a.Id,
            ScheduleId = a.ScheduleId,
            StudentId = a.StudentId,
            StudentName = string.Empty,
            IsPresent = a.IsPresent,
            Date = a.Date,
            Notes = a.Notes
        }).OrderByDescending(a => a.Date);
    }

    public async Task<AttendanceDto> MarkAsync(MarkAttendanceDto dto)
    {
        var existing = await _unitOfWork.Repository<Attendance>()
            .FindAsync(a => a.ScheduleId == dto.ScheduleId && a.StudentId == dto.StudentId && a.Date.Date == DateTime.UtcNow.Date);

        var record = existing.FirstOrDefault();
        if (record != null)
        {
            record.IsPresent = dto.IsPresent;
            record.Notes = dto.Notes;
            _unitOfWork.Repository<Attendance>().Update(record);
        }
        else
        {
            record = new Attendance
            {
                ScheduleId = dto.ScheduleId,
                StudentId = dto.StudentId,
                IsPresent = dto.IsPresent,
                Notes = dto.Notes,
                Date = DateTime.UtcNow
            };
            await _unitOfWork.Repository<Attendance>().AddAsync(record);
        }

        await _unitOfWork.SaveChangesAsync();
        _logger.LogInformation("Attendance marked: Student {StudentId} -> {Present}", dto.StudentId, dto.IsPresent);

        var student = await _unitOfWork.Repository<Student>().GetByIdAsync(dto.StudentId);
        return new AttendanceDto
        {
            Id = record.Id,
            ScheduleId = record.ScheduleId,
            StudentId = record.StudentId,
            StudentName = student?.FullName ?? "N/A",
            IsPresent = record.IsPresent,
            Date = record.Date,
            Notes = record.Notes
        };
    }

    public async Task MarkBulkAsync(BulkAttendanceDto dto)
    {
        var repo = _unitOfWork.Repository<Attendance>();
        foreach (var item in dto.Students)
        {
            var existing = await repo
                .FindAsync(a => a.ScheduleId == dto.ScheduleId && a.StudentId == item.StudentId && a.Date.Date == dto.Date.Date);

            var record = existing.FirstOrDefault();
            if (record != null)
            {
                record.IsPresent = item.IsPresent;
                record.Notes = item.Notes;
                repo.Update(record);
            }
            else
            {
                await repo.AddAsync(new Attendance
                {
                    ScheduleId = dto.ScheduleId,
                    StudentId = item.StudentId,
                    IsPresent = item.IsPresent,
                    Notes = item.Notes,
                    Date = dto.Date
                });
            }
        }
        await _unitOfWork.SaveChangesAsync();
        _logger.LogInformation("Bulk attendance saved for schedule {ScheduleId}", dto.ScheduleId);
    }

    public async Task<bool> HasAttendanceForDateAsync(int scheduleId, DateTime date)
    {
        return await _unitOfWork.Repository<Attendance>()
            .AnyAsync(a => a.ScheduleId == scheduleId && a.Date.Date == date.Date);
    }
}
