using Microsoft.EntityFrameworkCore;
using NotificationService.Api.Data;
using NotificationService.Api.Interfaces;
using NotificationService.Api.Models;

namespace NotificationService.Api.Repositories;

public class NotificationRepository : INotificationRepository
{
    private readonly NotificationDbContext _dbContext;

    public NotificationRepository(NotificationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<NotificationLog> AddAsync(NotificationLog notification, CancellationToken cancellationToken = default)
    {
        _dbContext.NotificationLogs.Add(notification);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return notification;
    }

    public async Task<IReadOnlyList<NotificationLog>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.NotificationLogs
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }
}
