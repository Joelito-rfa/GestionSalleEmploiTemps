using EMIT.Application.DTOs;
using EMIT.Application.Interfaces;
using EMIT.Domain.Entities;
using EMIT.Domain.Enums;
using EMIT.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace EMIT.Application.Services;

public class StudentService : IStudentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<StudentService> _logger;

    public StudentService(IUnitOfWork unitOfWork, ILogger<StudentService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<IEnumerable<StudentDto>> GetAllAsync()
    {
        var students = await _unitOfWork.Repository<Student>().GetAllAsync();
        return students.Select(MapToDto).OrderBy(s => s.FullName);
    }

    public async Task<IEnumerable<StudentDto>> GetByLevelAsync(StudentLevel level)
    {
        var students = await _unitOfWork.Repository<Student>().FindAsync(s => s.Level == level);
        return students.Select(MapToDto).OrderBy(s => s.FullName);
    }

    public async Task<StudentDto?> GetByIdAsync(int id)
    {
        var student = await _unitOfWork.Repository<Student>().GetByIdAsync(id);
        return student == null ? null : MapToDto(student);
    }

    public async Task<StudentDto> CreateAsync(CreateStudentDto dto)
    {
        var year = DateTime.UtcNow.Year;
        var existingMatricules = await _unitOfWork.Repository<Student>()
            .FindAsync(s => s.Matricule.StartsWith($"STU-{year}-"));
        var maxNum = existingMatricules
            .Select(s => int.Parse(s.Matricule.Split('-')[2]))
            .DefaultIfEmpty(0)
            .Max();
        var matricule = $"STU-{year}-{(maxNum + 1):D3}";

        var student = new Student
        {
            Matricule = matricule,
            FullName = dto.FullName,
            Email = dto.Email,
            Level = dto.Level
        };
        await _unitOfWork.Repository<Student>().AddAsync(student);
        await _unitOfWork.SaveChangesAsync();
        _logger.LogInformation("Student created: {Matricule} - {Email} ({Level})", student.Matricule, student.Email, student.Level);
        return MapToDto(student);
    }

    public async Task<StudentDto> UpdateAsync(UpdateStudentDto dto)
    {
        var repo = _unitOfWork.Repository<Student>();
        var student = await repo.GetByIdAsync(dto.Id) ?? throw new KeyNotFoundException($"Étudiant avec l'ID {dto.Id} introuvable.");
        student.FullName = dto.FullName;
        student.Email = dto.Email;
        student.Level = dto.Level;
        repo.Update(student);
        await _unitOfWork.SaveChangesAsync();
        _logger.LogInformation("Student updated: {Id} -> {Level}", student.Id, student.Level);
        return MapToDto(student);
    }

    public async Task DeleteAsync(int id)
    {
        var repo = _unitOfWork.Repository<Student>();
        var student = await repo.GetByIdAsync(id);
        if (student != null)
        {
            repo.Remove(student);
            await _unitOfWork.SaveChangesAsync();
            _logger.LogInformation("Student deleted: {Id}", id);
        }
    }

    public async Task<bool> ExistsAsync(int id) =>
        await _unitOfWork.Repository<Student>().AnyAsync(s => s.Id == id);

    public async Task<int> CountByLevelAsync() =>
        await _unitOfWork.Repository<Student>().CountAsync();

    public async Task<Dictionary<string, int>> GetStudentsByLevelAsync()
    {
        var students = await _unitOfWork.Repository<Student>().GetAllAsync();
        return students
            .GroupBy(s => s.Level.ToString())
            .ToDictionary(g => g.Key, g => g.Count());
    }

    private static StudentDto MapToDto(Student s) => new()
    {
        Id = s.Id,
        Matricule = s.Matricule,
        FullName = s.FullName,
        Email = s.Email,
        Level = s.Level,
        UserId = s.UserId
    };
}
