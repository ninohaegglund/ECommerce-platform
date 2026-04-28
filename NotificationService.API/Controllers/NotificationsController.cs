using Microsoft.AspNetCore.Mvc;
using NotificationService.Api.DTOs.Notifications;
using NotificationService.Api.Interfaces;
using NotificationService.Api.Models;

namespace NotificationService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpPost("order-confirmation")]
    public async Task<IActionResult> OrderConfirmation([FromBody] OrderConfirmationRequestDto request, CancellationToken cancellationToken)
    {
        var notification = await _notificationService.SendOrderConfirmationAsync(request, cancellationToken);
        return Ok(MapToResponse(notification));
    }

    [HttpPost("payment-confirmation")]
    public async Task<IActionResult> PaymentConfirmation([FromBody] PaymentConfirmationRequestDto request, CancellationToken cancellationToken)
    {
        var notification = await _notificationService.SendPaymentConfirmationAsync(request, cancellationToken);
        return Ok(MapToResponse(notification));
    }

    [HttpPost("payment-failed")]
    public async Task<IActionResult> PaymentFailed([FromBody] PaymentFailedRequestDto request, CancellationToken cancellationToken)
    {
        var notification = await _notificationService.SendPaymentFailedAsync(request, cancellationToken);
        return Ok(MapToResponse(notification));
    }

    [HttpGet("user/{userId:guid}")]
    public async Task<IActionResult> GetByUserId(Guid userId, CancellationToken cancellationToken)
    {
        var notifications = await _notificationService.GetByUserIdAsync(userId, cancellationToken);
        return Ok(notifications.Select(MapToResponse));
    }

    private static NotificationResponseDto MapToResponse(NotificationLog notification)
    {
        return new NotificationResponseDto
        {
            Id = notification.Id,
            UserId = notification.UserId,
            OrderId = notification.OrderId,
            Type = notification.Type,
            RecipientEmail = notification.RecipientEmail,
            Subject = notification.Subject,
            Body = notification.Body,
            SentAtUtc = notification.SentAtUtc,
            Status = notification.Status,
            CreatedAtUtc = notification.CreatedAtUtc
        };
    }
}
