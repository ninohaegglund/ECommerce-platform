using OrderService.Api.DTOs;

namespace OrderService.Api.Interfaces;

public interface IWishlistService
{
    Task<WishlistResponseDto> GetWishlistAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<WishlistResponseDto> AddItemAsync(Guid userId, AddWishlistItemDto request, CancellationToken cancellationToken = default);
    Task<bool> RemoveItemAsync(Guid userId, Guid itemId, CancellationToken cancellationToken = default);
}
