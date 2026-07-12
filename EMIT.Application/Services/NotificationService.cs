using EMIT.Application.DTOs;
using EMIT.Application.Interfaces;
using EMIT.Domain.Entities;
using EMIT.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace EMIT.Application.Services;

public class NotificationService : INotificationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(IUnitOfWork unitOfWork, ILogger<NotificationService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<IEnumerable<NotificationDto>> GetAllForUserAsync(string userId, string userRole, string? userLevel = null)
    {
        var repo = _unitOfWork.Repository<Notification>();

        if (userRole == "Student" && !string.IsNullOrEmpty(userLevel))
        {
            var notifications = await repo.FindAsync(n =>
                n.TargetRole == userRole &&
                (n.TargetLevel == null || n.TargetLevel == userLevel));
            return notifications.OrderByDescending(n => n.CreatedAt).Select(MapToDto);
        }

        var allNotifications = await repo.FindAsync(n => n.TargetRole == userRole);
        return allNotifications.OrderByDescending(n => n.CreatedAt).Select(MapToDto);
    }

    public async Task<NotificationDto?> GetByIdAsync(int id)
    {
        var notification = await _unitOfWork.Repository<Notification>().GetByIdAsync(id);
        return notification != null ? MapToDto(notification) : null;
    }

    public async Task<int> GetUnreadCountAsync(string userId, string userRole, string? userLevel = null)
    {
        var repo = _unitOfWork.Repository<Notification>();

        if (userRole == "Student" && !string.IsNullOrEmpty(userLevel))
        {
            return await repo.CountAsync(n =>
                n.TargetRole == userRole && !n.IsRead &&
                (n.TargetLevel == null || n.TargetLevel == userLevel));
        }

        return await repo.CountAsync(n => n.TargetRole == userRole && !n.IsRead);
    }

    public async Task MarkAsReadAsync(int id)
    {
        var repo = _unitOfWork.Repository<Notification>();
        var notification = await repo.GetByIdAsync(id);
        if (notification != null)
        {
            notification.IsRead = true;
            notification.UpdatedAt = DateTime.UtcNow;
            repo.Update(notification);
            await _unitOfWork.SaveChangesAsync();
        }
    }

    public async Task MarkAllAsReadAsync(string userId, string userRole, string? userLevel = null)
    {
        var repo = _unitOfWork.Repository<Notification>();
        IEnumerable<Notification> notifications;

        if (userRole == "Student" && !string.IsNullOrEmpty(userLevel))
        {
            notifications = await repo.FindAsync(n =>
                n.TargetRole == userRole && !n.IsRead &&
                (n.TargetLevel == null || n.TargetLevel == userLevel));
        }
        else
        {
            notifications = await repo.FindAsync(n => n.TargetRole == userRole && !n.IsRead);
        }

        foreach (var notification in notifications)
        {
            notification.IsRead = true;
            notification.UpdatedAt = DateTime.UtcNow;
            repo.Update(notification);
        }
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<NotificationDto> CreateAsync(CreateNotificationDto dto)
    {
        var notification = new Notification
        {
            Title = dto.Title,
            Message = dto.Message,
            Type = dto.Type,
            IsRead = false,
            TargetRole = dto.TargetRole,
            TargetLevel = dto.TargetLevel,
            CreatedByUserId = dto.CreatedByUserId
        };

        await _unitOfWork.Repository<Notification>().AddAsync(notification);
        await _unitOfWork.SaveChangesAsync();
        _logger.LogInformation("Notification created: {Title}", notification.Title);

        return MapToDto(notification);
    }

    public async Task DeleteAsync(int id)
    {
        var repo = _unitOfWork.Repository<Notification>();
        var notification = await repo.GetByIdAsync(id);
        if (notification != null)
        {
            repo.Remove(notification);
            await _unitOfWork.SaveChangesAsync();
            _logger.LogInformation("Notification deleted: {Id}", id);
        }
    }

    public async Task DeleteAllForUserAsync(string userId, string userRole, string? userLevel = null)
    {
        var repo = _unitOfWork.Repository<Notification>();
        IEnumerable<Notification> notifications;

        if (userRole == "Student" && !string.IsNullOrEmpty(userLevel))
        {
            notifications = await repo.FindAsync(n =>
                n.TargetRole == userRole &&
                (n.TargetLevel == null || n.TargetLevel == userLevel));
        }
        else
        {
            notifications = await repo.FindAsync(n => n.TargetRole == userRole);
        }

        var list = notifications.ToList();
        if (list.Count > 0)
        {
            repo.RemoveRange(list);
            await _unitOfWork.SaveChangesAsync();
            _logger.LogInformation("Deleted {Count} notifications for role {Role}", list.Count, userRole);
        }
    }

    private static NotificationDto MapToDto(Notification notification) => new()
    {
        Id = notification.Id,
        Title = notification.Title,
        Message = notification.Message,
        Type = notification.Type,
        IsRead = notification.IsRead,
        TargetRole = notification.TargetRole,
        TargetLevel = notification.TargetLevel,
        CreatedByUserId = notification.CreatedByUserId,
        CreatedAt = notification.CreatedAt
    };
}
