using EMIT.Application.DTOs;

namespace EMIT.Application.Interfaces;

public interface INotificationService
{
    Task<IEnumerable<NotificationDto>> GetAllForUserAsync(string userId, string userRole, string? userLevel = null);
    Task<NotificationDto?> GetByIdAsync(int id);
    Task<int> GetUnreadCountAsync(string userId, string userRole, string? userLevel = null);
    Task MarkAsReadAsync(int id);
    Task MarkAllAsReadAsync(string userId, string userRole, string? userLevel = null);
    Task<NotificationDto> CreateAsync(CreateNotificationDto dto);
    Task DeleteAsync(int id);
    Task DeleteAllForUserAsync(string userId, string userRole, string? userLevel = null);
}
