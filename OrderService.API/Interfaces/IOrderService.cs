using OrderService.Api.DTOs;
using OrderService.Api.Models;

namespace OrderService.Api.Interfaces;

public interface IOrderService
{
    Task<IReadOnlyList<OrderResponseDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<OrderResponseDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OrderResponseDto>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<OrderResponseDto?> UpdateStatusAsync(Guid id, OrderStatus status, CancellationToken cancellationToken = default);
    Task<OrderResponseDto?> UpdatePaymentAsync(Guid id, UpdateOrderPaymentRequestDto request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
