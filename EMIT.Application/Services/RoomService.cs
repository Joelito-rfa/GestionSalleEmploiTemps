using EMIT.Application.DTOs;
using EMIT.Application.Interfaces;
using EMIT.Domain.Entities;
using EMIT.Domain.Interfaces;

namespace EMIT.Application.Services;

public class RoomService : IRoomService
{
    private readonly IUnitOfWork _unitOfWork;

    public RoomService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<RoomDto>> GetAllAsync()
    {
        var rooms = await _unitOfWork.Repository<Room>().GetAllAsync();
        return rooms.Select(MapToDto);
    }

    public async Task<RoomDto?> GetByIdAsync(int id)
    {
        var room = await _unitOfWork.Repository<Room>().GetByIdAsync(id);
        return room == null ? null : MapToDto(room);
    }

    public async Task<RoomDto> CreateAsync(CreateRoomDto dto)
    {
        var room = new Room
        {
            Name = dto.Name,
            Capacity = dto.Capacity,
            Location = dto.Location
        };
        await _unitOfWork.Repository<Room>().AddAsync(room);
        await _unitOfWork.SaveChangesAsync();
        return MapToDto(room);
    }

    public async Task<RoomDto> UpdateAsync(UpdateRoomDto dto)
    {
        var repo = _unitOfWork.Repository<Room>();
        var room = await repo.GetByIdAsync(dto.Id) ?? throw new KeyNotFoundException($"Salle avec l'ID {dto.Id} introuvable.");
        room.Name = dto.Name;
        room.Capacity = dto.Capacity;
        room.Location = dto.Location;
        repo.Update(room);
        await _unitOfWork.SaveChangesAsync();
        return MapToDto(room);
    }

    public async Task DeleteAsync(int id)
    {
        var repo = _unitOfWork.Repository<Room>();
        var room = await repo.GetByIdAsync(id);
        if (room != null)
        {
            repo.Remove(room);
            await _unitOfWork.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(int id) =>
        await _unitOfWork.Repository<Room>().AnyAsync(r => r.Id == id);

    private static RoomDto MapToDto(Room room) => new()
    {
        Id = room.Id,
        Name = room.Name,
        Capacity = room.Capacity,
        Location = room.Location
    };
}
