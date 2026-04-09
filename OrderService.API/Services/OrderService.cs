using OrderService.Api.DTOs;
using OrderService.Api.Interfaces;
using OrderService.Api.Models;

namespace OrderService.Api.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;

    public OrderService(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public Task<IReadOnlyList<Order>> GetAllAsync(CancellationToken cancellationToken = default)
        => _orderRepository.GetAllAsync(cancellationToken);

    public Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _orderRepository.GetByIdAsync(id, cancellationToken);

    public Task<IReadOnlyList<Order>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default)
        => _orderRepository.GetByCustomerIdAsync(customerId, cancellationToken);

    public async Task<Order> CreateAsync(CreateOrderRequestDto request, CancellationToken cancellationToken = default)
    {
        var items = request.Items.Select(x =>
        {
            var total = (x.UnitPrice * x.Quantity) - x.DiscountAmount;
            return new OrderItem
            {
                ProductId = x.ProductId,
                ProductName = x.ProductName,
                Sku = x.Sku,
                Quantity = x.Quantity,
                UnitPrice = x.UnitPrice,
                DiscountAmount = x.DiscountAmount,
                TotalPrice = total < 0 ? 0 : total
            };
        }).ToList();

        var subtotal = items.Sum(x => x.UnitPrice * x.Quantity);
        var discount = items.Sum(x => x.DiscountAmount);
        const decimal shipping = 0m;
        const decimal tax = 0m;
        var totalAmount = subtotal + shipping + tax - discount;

        var order = new Order
        {
            CustomerId = request.CustomerId,
            OrderNumber = $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Random.Shared.Next(1000, 9999)}",
            Status = OrderStatus.Pending,
            Currency = request.Currency,
            SubtotalAmount = subtotal,
            ShippingAmount = shipping,
            TaxAmount = tax,
            DiscountAmount = discount,
            TotalAmount = totalAmount < 0 ? 0 : totalAmount,
            ShippingAddress = new Address
            {
                FirstName = request.ShippingAddress.FirstName,
                LastName = request.ShippingAddress.LastName,
                Company = request.ShippingAddress.Company,
                StreetLine1 = request.ShippingAddress.StreetLine1,
                StreetLine2 = request.ShippingAddress.StreetLine2,
                City = request.ShippingAddress.City,
                PostalCode = request.ShippingAddress.PostalCode,
                Region = request.ShippingAddress.Region,
                CountryCode = request.ShippingAddress.CountryCode,
                PhoneNumber = request.ShippingAddress.PhoneNumber
            },
            BillingAddress = new Address
            {
                FirstName = request.BillingAddress.FirstName,
                LastName = request.BillingAddress.LastName,
                Company = request.BillingAddress.Company,
                StreetLine1 = request.BillingAddress.StreetLine1,
                StreetLine2 = request.BillingAddress.StreetLine2,
                City = request.BillingAddress.City,
                PostalCode = request.BillingAddress.PostalCode,
                Region = request.BillingAddress.Region,
                CountryCode = request.BillingAddress.CountryCode,
                PhoneNumber = request.BillingAddress.PhoneNumber
            },
            Payment = new Payment
            {
                Method = request.PaymentMethod,
                Provider = request.PaymentProvider,
                TransactionId = request.PaymentTransactionId,
                Currency = request.Currency,
                Amount = totalAmount < 0 ? 0 : totalAmount,
                Status = PaymentStatus.NotPaid
            },
            Items = items
        };

        return await _orderRepository.AddAsync(order, cancellationToken);
    }

    public async Task<Order?> UpdateStatusAsync(Guid id, OrderStatus status, CancellationToken cancellationToken = default)
    {
        var existing = await _orderRepository.GetByIdAsync(id, cancellationToken);
        if (existing is null)
        {
            return null;
        }

        existing.Status = status;
        existing.UpdatedAtUtc = DateTime.UtcNow;
        return await _orderRepository.UpdateAsync(existing, cancellationToken);
    }

    public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        => _orderRepository.DeleteAsync(id, cancellationToken);
}