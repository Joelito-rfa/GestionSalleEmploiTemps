using EMIT.Application.DTOs;
using EMIT.Application.Interfaces;
using EMIT.Domain.Entities;
using EMIT.Domain.Interfaces;

namespace EMIT.Application.Services;

public class TeacherService : ITeacherService
{
    private readonly IUnitOfWork _unitOfWork;

    public TeacherService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<TeacherDto>> GetAllAsync()
    {
        var teachers = await _unitOfWork.Repository<Teacher>().GetAllIncludingAsync(t => t.Schedules);
        return teachers.Select(t => MapToDto(t));
    }

    public async Task<TeacherDto?> GetByIdAsync(int id)
    {
        var teachers = await _unitOfWork.Repository<Teacher>().FindIncludingAsync(t => t.Id == id, t => t.Schedules);
        var teacher = teachers.FirstOrDefault();
        return teacher == null ? null : MapToDto(teacher);
    }

    public async Task<TeacherDto> CreateAsync(CreateTeacherDto dto)
    {
        var year = DateTime.UtcNow.Year;
        var existingNumeros = await _unitOfWork.Repository<Teacher>()
            .FindAsync(t => t.Numero.StartsWith($"PROF-{year}-"));
        var maxNum = existingNumeros
            .Select(t => int.Parse(t.Numero.Split('-')[2]))
            .DefaultIfEmpty(0)
            .Max();
        var numero = $"PROF-{year}-{(maxNum + 1):D3}";

        var teacher = new Teacher
        {
            Numero = numero,
            FullName = dto.FullName,
            Email = dto.Email,
            Subject = dto.Subject
        };
        await _unitOfWork.Repository<Teacher>().AddAsync(teacher);
        await _unitOfWork.SaveChangesAsync();
        return MapToDto(teacher);
    }

    public async Task<TeacherDto> UpdateAsync(UpdateTeacherDto dto)
    {
        var repo = _unitOfWork.Repository<Teacher>();
        var teachers = await repo.FindIncludingAsync(t => t.Id == dto.Id, t => t.Schedules);
        var teacher = teachers.FirstOrDefault() ?? throw new KeyNotFoundException($"Enseignant avec l'ID {dto.Id} introuvable.");
        teacher.FullName = dto.FullName;
        teacher.Email = dto.Email;
        teacher.Subject = dto.Subject;
        repo.Update(teacher);
        await _unitOfWork.SaveChangesAsync();
        return MapToDto(teacher);
    }

    public async Task DeleteAsync(int id)
    {
        var repo = _unitOfWork.Repository<Teacher>();
        var teacher = await repo.GetByIdAsync(id);
        if (teacher != null)
        {
            repo.Remove(teacher);
            await _unitOfWork.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(int id) =>
        await _unitOfWork.Repository<Teacher>().AnyAsync(t => t.Id == id);

    private static TeacherDto MapToDto(Teacher teacher) => new()
    {
        Id = teacher.Id,
        Numero = teacher.Numero,
        FullName = teacher.FullName,
        Email = teacher.Email,
        Subject = teacher.Subject,
        ScheduleCount = teacher.Schedules?.Count ?? 0
    };
}
