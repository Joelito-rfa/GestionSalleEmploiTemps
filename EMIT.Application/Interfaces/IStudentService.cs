using EMIT.Application.DTOs;
using EMIT.Domain.Enums;

namespace EMIT.Application.Interfaces;

public interface IStudentService
{
    Task<IEnumerable<StudentDto>> GetAllAsync();
    Task<IEnumerable<StudentDto>> GetByLevelAsync(StudentLevel level);
    Task<StudentDto?> GetByIdAsync(int id);
    Task<StudentDto> CreateAsync(CreateStudentDto dto);
    Task<StudentDto> UpdateAsync(UpdateStudentDto dto);
    Task DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<int> CountByLevelAsync();
    Task<Dictionary<string, int>> GetStudentsByLevelAsync();
}
