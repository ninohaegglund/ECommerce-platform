using OrderService.Api.DTOs;
using OrderService.Api.Interfaces;
using OrderService.Api.Models;
using System.Text.Json;

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

    public Task<IReadOnlyList<Order>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        => _orderRepository.GetByUserIdAsync(userId, cancellationToken);

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

    public async Task<Order?> UpdatePaymentAsync(Guid id, UpdateOrderPaymentRequestDto request, CancellationToken cancellationToken = default)
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
        return await _orderRepository.UpdateAsync(existing, cancellationToken);
    }

    public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        => _orderRepository.DeleteAsync(id, cancellationToken);

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
