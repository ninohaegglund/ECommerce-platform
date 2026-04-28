using NotificationService.Api.Models;

namespace NotificationService.Api.Interfaces;

public interface INotificationRepository
{
    Task<NotificationLog> AddAsync(NotificationLog notification, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<NotificationLog>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}
