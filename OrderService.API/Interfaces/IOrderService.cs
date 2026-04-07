using OrderService.Api.DTOs;
using OrderService.Api.Models;

namespace OrderService.Api.Interfaces;

public interface IOrderService
{
    Task<IReadOnlyList<Order>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Order>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<Order> CreateAsync(CreateOrderRequestDto request, CancellationToken cancellationToken = default);
    Task<Order?> UpdateStatusAsync(Guid id, OrderStatus status, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}