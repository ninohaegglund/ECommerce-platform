using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NotificationService.Api.Controllers;
using NotificationService.Api.DTOs.Newsletter;
using NotificationService.Api.DTOs.Notifications;
using NotificationService.Api.Interfaces;
using NotificationService.Api.Models;

namespace ECommerceMicroservices.Tests.Controllers;

public class NotificationApiControllerTests
{
    [Fact]
    public async Task PaymentFailed_ReturnsBadGateway_WhenNotificationFails()
    {
        var request = new PaymentFailedRequestDto
        {
            UserId = Guid.NewGuid(),
            OrderId = Guid.NewGuid(),
            RecipientEmail = "customer@test.local",
            OrderNumber = "ORD-1002",
            FailureReason = "Card declined"
        };
        var log = new NotificationLog
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            OrderId = request.OrderId,
            Type = NotificationType.PaymentFailed,
            RecipientEmail = request.RecipientEmail,
            Status = NotificationStatus.Failed,
            FailureReason = "SMTP timeout"
        };

        var notificationService = new Mock<INotificationService>();
        notificationService
            .Setup(x => x.SendPaymentFailedAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(log);

        var controller = new NotificationsController(notificationService.Object);

        var result = await controller.PaymentFailed(request, CancellationToken.None);

        var objectResult = result.Should().BeOfType<ObjectResult>().Which;
        objectResult.StatusCode.Should().Be(502);
        var response = objectResult.Value.Should().BeOfType<NotificationResponseDto>().Which;
        response.Id.Should().Be(log.Id);
        response.Status.Should().Be(NotificationStatus.Failed);
        response.FailureReason.Should().Be("SMTP timeout");
    }

    [Fact]
    public async Task GetByUserId_ReturnsNotificationsForUser()
    {
        var userId = Guid.NewGuid();
        var logs = new List<NotificationLog>
        {
            new()
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Type = NotificationType.AccountCreated,
                RecipientEmail = "customer@test.local",
                Status = NotificationStatus.Sent
            }
        };

        var notificationService = new Mock<INotificationService>();
        notificationService
            .Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(logs);

        var controller = new NotificationsController(notificationService.Object);

        var result = await controller.GetByUserId(userId, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Which;
        var notifications = Assert.IsAssignableFrom<IEnumerable<NotificationResponseDto>>(ok.Value);
        notifications.Should().ContainSingle()
            .Which.RecipientEmail.Should().Be("customer@test.local");
    }

    [Fact]
    public async Task Newsletter_Subscribe_ReturnsSubscriber()
    {
        var request = new SubscribeNewsletterRequestDto
        {
            Email = "subscriber@test.local",
            FirstName = "Linus",
            LastName = "Torvalds"
        };
        var subscriber = new NewsletterSubscriber
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            IsSubscribed = true
        };

        var newsletterService = new Mock<INewsletterService>();
        newsletterService
            .Setup(x => x.SubscribeAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscriber);

        var controller = new NewsletterController(newsletterService.Object);

        var result = await controller.Subscribe(request, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Which;
        var response = ok.Value.Should().BeOfType<NewsletterSubscriberResponseDto>().Which;
        response.Id.Should().Be(subscriber.Id);
        response.Email.Should().Be(request.Email);
        response.IsSubscribed.Should().BeTrue();
    }

    [Fact]
    public async Task Newsletter_Unsubscribe_ReturnsNotFound_WhenSubscriberDoesNotExist()
    {
        var request = new UnsubscribeNewsletterRequestDto
        {
            Email = "missing@test.local"
        };

        var newsletterService = new Mock<INewsletterService>();
        newsletterService
            .Setup(x => x.UnsubscribeAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync((NewsletterSubscriber?)null);

        var controller = new NewsletterController(newsletterService.Object);

        var result = await controller.Unsubscribe(request, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }
}
