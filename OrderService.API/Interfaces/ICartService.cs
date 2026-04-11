using OrderService.Api.DTOs;
using OrderService.Api.Models;

namespace OrderService.Api.Interfaces;

public interface ICartService
{
    Task<CartResponseDto> GetCartAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<CartResponseDto> AddItemAsync(Guid userId, AddCartItemDto request, CancellationToken cancellationToken = default);
    Task<CartResponseDto?> UpdateItemQuantityAsync(Guid userId, Guid itemId, UpdateCartItemQuantityDto request, CancellationToken cancellationToken = default);
    Task<bool> RemoveItemAsync(Guid userId, Guid itemId, CancellationToken cancellationToken = default);
    Task<Order> CheckoutAsync(Guid userId, CheckoutCartRequestDto request, CancellationToken cancellationToken = default);
}
