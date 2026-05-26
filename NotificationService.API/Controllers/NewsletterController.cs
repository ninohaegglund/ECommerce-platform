using Microsoft.AspNetCore.Mvc;
using NotificationService.Api.DTOs.Newsletter;
using NotificationService.Api.Interfaces;
using NotificationService.Api.Models;

namespace NotificationService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NewsletterController : ControllerBase
{
    private readonly INewsletterService _newsletterService;

    public NewsletterController(INewsletterService newsletterService)
    {
        _newsletterService = newsletterService;
    }

    [HttpPost("subscribe")]
    public async Task<IActionResult> Subscribe([FromBody] SubscribeNewsletterRequestDto request, CancellationToken cancellationToken)
    {
        var subscriber = await _newsletterService.SubscribeAsync(request, cancellationToken);
        return Ok(MapToResponse(subscriber));
    }

    [HttpPost("unsubscribe")]
    public async Task<IActionResult> Unsubscribe([FromBody] UnsubscribeNewsletterRequestDto request, CancellationToken cancellationToken)
    {
        var subscriber = await _newsletterService.UnsubscribeAsync(request, cancellationToken);
        return subscriber is null ? NotFound() : Ok(MapToResponse(subscriber));
    }

    [HttpGet("subscribers")]
    public async Task<IActionResult> GetSubscribers(
        [FromQuery] bool includeUnsubscribed = false,
        CancellationToken cancellationToken = default)
    {
        var subscribers = await _newsletterService.GetSubscribersAsync(includeUnsubscribed, cancellationToken);
        return Ok(subscribers.Select(MapToResponse));
    }

    [HttpPost("send")]
    public async Task<IActionResult> Send([FromBody] SendNewsletterRequestDto request, CancellationToken cancellationToken)
    {
        var response = await _newsletterService.SendAsync(request, cancellationToken);
        return Ok(response);
    }

    private static NewsletterSubscriberResponseDto MapToResponse(NewsletterSubscriber subscriber)
    {
        return new NewsletterSubscriberResponseDto
        {
            Id = subscriber.Id,
            Email = subscriber.Email,
            FirstName = subscriber.FirstName,
            LastName = subscriber.LastName,
            IsSubscribed = subscriber.IsSubscribed,
            SubscribedAtUtc = subscriber.SubscribedAtUtc,
            UnsubscribedAtUtc = subscriber.UnsubscribedAtUtc,
            CreatedAtUtc = subscriber.CreatedAtUtc,
            UpdatedAtUtc = subscriber.UpdatedAtUtc
        };
    }
}
