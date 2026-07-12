using EMIT.Application.DTOs;

namespace EMIT.Application.Interfaces;

public interface IProfileService
{
    Task<ProfileDto> GetProfileAsync(string userId);
    Task UpdateProfileAsync(string userId, UpdateProfileDto dto);
}
