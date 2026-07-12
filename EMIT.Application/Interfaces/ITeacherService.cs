using EMIT.Application.DTOs;

namespace EMIT.Application.Interfaces;

public interface ITeacherService
{
    Task<IEnumerable<TeacherDto>> GetAllAsync();
    Task<TeacherDto?> GetByIdAsync(int id);
    Task<TeacherDto> CreateAsync(CreateTeacherDto dto);
    Task<TeacherDto> UpdateAsync(UpdateTeacherDto dto);
    Task DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
}
