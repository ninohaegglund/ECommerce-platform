namespace NotificationService.Api.Interfaces;

public interface IEmailSender
{
    Task<EmailSendResult> SendAsync(EmailSendRequest request, CancellationToken cancellationToken = default);
}

public sealed record EmailSendRequest(
    string To,
    string Subject,
    string TextBody,
    string? HtmlBody = null,
    string? IdempotencyKey = null);

public sealed record EmailSendResult(
    string Provider,
    string? MessageId);
