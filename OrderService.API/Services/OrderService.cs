using OrderService.Api.DTOs;
using OrderService.Api.Interfaces;
using OrderService.Api.Models;
using System.Text.Json;

namespace OrderService.Api.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IIdentityUserClient _identityUserClient;

    public OrderService(IOrderRepository orderRepository, IIdentityUserClient identityUserClient)
    {
        _orderRepository = orderRepository;
        _identityUserClient = identityUserClient;
    }

    public async Task<IReadOnlyList<OrderResponseDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var orders = await _orderRepository.GetAllAsync(cancellationToken);
        return await MapOrdersAsync(orders, cancellationToken);
    }

    public async Task<OrderResponseDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetByIdAsync(id, cancellationToken);
        return order is null
            ? null
            : await MapOrderAsync(order, cancellationToken);
    }

    public async Task<IReadOnlyList<OrderResponseDto>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var orders = await _orderRepository.GetByUserIdAsync(userId, cancellationToken);
        return await MapOrdersAsync(orders, cancellationToken);
    }

    public async Task<OrderResponseDto?> UpdateStatusAsync(Guid id, OrderStatus status, CancellationToken cancellationToken = default)
    {
        var existing = await _orderRepository.GetByIdAsync(id, cancellationToken);
        if (existing is null)
        {
            return null;
        }

        existing.Status = status;
        existing.UpdatedAtUtc = DateTime.UtcNow;
        var updated = await _orderRepository.UpdateAsync(existing, cancellationToken);
        return await MapOrderAsync(updated, cancellationToken);
    }

    public async Task<OrderResponseDto?> UpdatePaymentAsync(Guid id, UpdateOrderPaymentRequestDto request, CancellationToken cancellationToken = default)
    {
        if (!TryParsePaymentStatus(request.PaymentStatus ?? request.Status, out var paymentStatus, out var rawPaymentStatus))
        {
            throw new InvalidOperationException(string.IsNullOrWhiteSpace(rawPaymentStatus)
                ? "PaymentStatus is required."
                : $"PaymentStatus '{rawPaymentStatus}' is not supported.");
        }

        OrderStatus? orderStatus = null;
        if (!string.IsNullOrWhiteSpace(request.OrderStatus))
        {
            if (!Enum.TryParse<OrderStatus>(request.OrderStatus, ignoreCase: true, out var parsedOrderStatus))
            {
                throw new InvalidOperationException($"OrderStatus '{request.OrderStatus}' is not supported.");
            }

            orderStatus = parsedOrderStatus;
        }

        var existing = await _orderRepository.GetByIdAsync(id, cancellationToken);
        if (existing is null)
        {
            return null;
        }

        existing.PaymentStatus = paymentStatus;

        var transactionId = request.PaymentTransactionId ?? request.TransactionId;
        if (transactionId is not null)
        {
            existing.PaymentTransactionId = string.IsNullOrWhiteSpace(transactionId)
                ? null
                : transactionId;
        }

        var provider = request.PaymentProvider ?? request.Provider;
        if (provider is not null)
        {
            existing.PaymentProvider = string.IsNullOrWhiteSpace(provider)
                ? null
                : provider;
        }

        existing.Status = orderStatus ?? InferOrderStatus(existing.Status, paymentStatus);

        existing.UpdatedAtUtc = DateTime.UtcNow;
        var updated = await _orderRepository.UpdateAsync(existing, cancellationToken);
        return await MapOrderAsync(updated, cancellationToken);
    }

    public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        => _orderRepository.DeleteAsync(id, cancellationToken);

    private async Task<OrderResponseDto> MapOrderAsync(Order order, CancellationToken cancellationToken)
    {
        var customerEmail = await _identityUserClient.GetUserEmailAsync(order.UserId, cancellationToken);
        return MapOrderResponse(order, customerEmail);
    }

    private async Task<IReadOnlyList<OrderResponseDto>> MapOrdersAsync(
        IReadOnlyList<Order> orders,
        CancellationToken cancellationToken)
    {
        var customerEmails = await GetCustomerEmailsAsync(orders, cancellationToken);

        return orders
            .Select(order => MapOrderResponse(
                order,
                customerEmails.GetValueOrDefault(order.UserId)))
            .ToList();
    }

    private async Task<Dictionary<Guid, string?>> GetCustomerEmailsAsync(
        IReadOnlyList<Order> orders,
        CancellationToken cancellationToken)
    {
        var userIds = orders
            .Select(order => order.UserId)
            .Distinct()
            .ToList();

        var emailTasks = userIds.ToDictionary(
            userId => userId,
            userId => _identityUserClient.GetUserEmailAsync(userId, cancellationToken));

        await Task.WhenAll(emailTasks.Values);

        return emailTasks.ToDictionary(
            pair => pair.Key,
            pair => pair.Value.Result);
    }

    private static OrderResponseDto MapOrderResponse(Order order, string? customerEmail)
    {
        return new OrderResponseDto
        {
            Id = order.Id,
            UserId = order.UserId,
            CustomerEmail = customerEmail,
            OrderNumber = order.OrderNumber,
            Status = order.Status,
            SubtotalAmount = order.SubtotalAmount,
            ShippingAmount = order.ShippingAmount,
            TaxAmount = order.TaxAmount,
            DiscountAmount = order.DiscountAmount,
            TotalAmount = order.TotalAmount,
            Currency = order.Currency,
            CreatedAtUtc = order.CreatedAtUtc,
            UpdatedAtUtc = order.UpdatedAtUtc,
            ShippingAddress = MapAddressResponse(order.ShippingAddress),
            BillingAddress = MapAddressResponse(order.BillingAddress),
            PaymentStatus = order.PaymentStatus,
            PaymentTransactionId = order.PaymentTransactionId,
            PaymentProvider = order.PaymentProvider,
            Items = order.Items.Select(MapOrderItemResponse).ToList()
        };
    }

    private static AddressResponseDto MapAddressResponse(Address address)
    {
        return new AddressResponseDto
        {
            FirstName = address.FirstName,
            LastName = address.LastName,
            Company = address.Company,
            StreetLine1 = address.StreetLine1,
            StreetLine2 = address.StreetLine2,
            City = address.City,
            PostalCode = address.PostalCode,
            Region = address.Region,
            CountryCode = address.CountryCode,
            PhoneNumber = address.PhoneNumber
        };
    }

    private static OrderItemResponseDto MapOrderItemResponse(OrderItem item)
    {
        return new OrderItemResponseDto
        {
            Id = item.Id,
            ProductId = item.ProductId,
            ProductName = item.ProductName,
            Sku = item.Sku,
            Quantity = item.Quantity,
            UnitPrice = item.UnitPrice,
            DiscountAmount = item.DiscountAmount,
            TotalPrice = item.TotalPrice
        };
    }

    private static OrderStatus InferOrderStatus(OrderStatus currentStatus, PaymentStatus paymentStatus)
    {
        return paymentStatus switch
        {
            PaymentStatus.Paid => OrderStatus.Paid,
            PaymentStatus.Cancelled => OrderStatus.Cancelled,
            PaymentStatus.Refunded => OrderStatus.Refunded,
            _ => currentStatus
        };
    }

    private static bool TryParsePaymentStatus(JsonElement? value, out PaymentStatus paymentStatus, out string? rawStatus)
    {
        paymentStatus = default;
        rawStatus = null;

        if (!value.HasValue || value.Value.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
        {
            return false;
        }

        if (value.Value.ValueKind == JsonValueKind.Number)
        {
            if (!value.Value.TryGetInt32(out var numericStatus))
            {
                rawStatus = value.Value.GetRawText();
                return false;
            }

            rawStatus = numericStatus.ToString();
            return TryMapPaymentStatus(numericStatus, out paymentStatus);
        }

        if (value.Value.ValueKind != JsonValueKind.String)
        {
            rawStatus = value.Value.GetRawText();
            return false;
        }

        rawStatus = value.Value.GetString();
        return TryMapPaymentStatus(rawStatus, out paymentStatus);
    }

    private static bool TryMapPaymentStatus(int status, out PaymentStatus paymentStatus)
    {
        paymentStatus = status switch
        {
            0 => PaymentStatus.NotPaid,
            1 => PaymentStatus.Paid,
            2 => PaymentStatus.Failed,
            3 => PaymentStatus.Cancelled,
            4 => PaymentStatus.Refunded,
            _ => default
        };

        return status is >= 0 and <= 4;
    }

    private static bool TryMapPaymentStatus(string? status, out PaymentStatus paymentStatus)
    {
        paymentStatus = status?.Trim().ToLowerInvariant() switch
        {
            "pending" or "notpaid" or "not_paid" or "unpaid" => PaymentStatus.NotPaid,
            "captured" or "paid" or "succeeded" or "success" => PaymentStatus.Paid,
            "failed" or "payment_failed" => PaymentStatus.Failed,
            "cancelled" or "canceled" => PaymentStatus.Cancelled,
            "refunded" => PaymentStatus.Refunded,
            _ => default
        };

        return status?.Trim().ToLowerInvariant() is
            "pending" or "notpaid" or "not_paid" or "unpaid" or
            "captured" or "paid" or "succeeded" or "success" or
            "failed" or "payment_failed" or
            "cancelled" or "canceled" or
            "refunded";
    }
}
