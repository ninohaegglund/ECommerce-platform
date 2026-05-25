namespace NotificationService.Api.Options;

public class ResendOptions
{
    public const string SectionName = "Resend";

    public string ApiBaseUrl { get; set; } = "https://api.resend.com";
    public string ApiKey { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = "ECommerce Platform";
}
