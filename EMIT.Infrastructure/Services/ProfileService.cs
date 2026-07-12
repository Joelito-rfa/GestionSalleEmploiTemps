using EMIT.Application.DTOs;
using EMIT.Application.Interfaces;
using EMIT.Domain.Entities;
using EMIT.Domain.Interfaces;
using EMIT.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace EMIT.Infrastructure.Services;

public class ProfileService : IProfileService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ProfileService> _logger;

    public ProfileService(
        UserManager<ApplicationUser> userManager,
        IUnitOfWork unitOfWork,
        ILogger<ProfileService> logger)
    {
        _userManager = userManager;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<ProfileDto> GetProfileAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new KeyNotFoundException("Utilisateur introuvable.");

        var scheduleRepo = _unitOfWork.Repository<Schedule>();
        var attendanceRepo = _unitOfWork.Repository<Attendance>();
        int totalSchedules;
        int totalAttendances;

        if (user.Role == "Teacher")
        {
            var teacher = _unitOfWork.Repository<Teacher>().Query()
                .FirstOrDefault(t => t.UserId == userId);
            if (teacher != null)
            {
                totalSchedules = await scheduleRepo.CountAsync(s => s.TeacherId == teacher.Id);
                var scheduleIds = (await scheduleRepo.FindAsync(s => s.TeacherId == teacher.Id))
                    .Select(s => s.Id).ToList();
                totalAttendances = await attendanceRepo.CountAsync(a => scheduleIds.Contains(a.ScheduleId));
            }
            else
            {
                totalSchedules = 0;
                totalAttendances = 0;
            }
        }
        else if (user.Role == "Student")
        {
            var student = _unitOfWork.Repository<Student>().Query()
                .FirstOrDefault(s => s.UserId == userId);
            if (student != null)
            {
                totalSchedules = await scheduleRepo.CountAsync(s => s.Level == student.Level);
                totalAttendances = await attendanceRepo.CountAsync(a => a.StudentId == student.Id);
            }
            else
            {
                totalSchedules = 0;
                totalAttendances = 0;
            }
        }
        else
        {
            totalSchedules = await scheduleRepo.CountAsync();
            totalAttendances = await attendanceRepo.CountAsync();
        }

        return new ProfileDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email ?? "",
            Role = user.Role,
            ProfilePicturePath = user.ProfilePicturePath,
            CreatedAt = user.CreatedAt,
            TotalSchedules = totalSchedules,
            TotalAttendances = totalAttendances
        };
    }

    public async Task UpdateProfileAsync(string userId, UpdateProfileDto dto)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new KeyNotFoundException("Utilisateur introuvable.");

        user.FullName = $"{dto.FirstName} {dto.LastName}";
        user.Email = dto.Email;
        user.UserName = dto.Email;

        if (dto.ProfilePicturePath != null)
        {
            if (!string.IsNullOrEmpty(user.ProfilePicturePath))
            {
                var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot",
                    user.ProfilePicturePath.TrimStart('/'));
                if (File.Exists(oldPath))
                    File.Delete(oldPath);
            }
            user.ProfilePicturePath = dto.ProfilePicturePath;
        }

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                string.Join("; ", result.Errors.Select(e => e.Description)));
        }

        _logger.LogInformation("Profile updated for user {UserId}", userId);
    }
}
