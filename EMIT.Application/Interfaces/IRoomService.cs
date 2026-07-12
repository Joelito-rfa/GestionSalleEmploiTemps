using EMIT.Application.DTOs;

namespace EMIT.Application.Interfaces;

public interface IRoomService
{
    Task<IEnumerable<RoomDto>> GetAllAsync();
    Task<RoomDto?> GetByIdAsync(int id);
    Task<RoomDto> CreateAsync(CreateRoomDto dto);
    Task<RoomDto> UpdateAsync(UpdateRoomDto dto);
    Task DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
}
