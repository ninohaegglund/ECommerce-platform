namespace OrderService.Api.Interfaces;

public interface IIdentityUserClient
{
    Task<string?> GetUserEmailAsync(Guid userId, CancellationToken cancellationToken = default);
}
