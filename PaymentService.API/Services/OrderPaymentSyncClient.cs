using System.Net.Http.Json;
using PaymentService.Api.DTOs;
using PaymentService.Api.Interfaces;
using PaymentService.Api.Models;

namespace PaymentService.Api.Services;

public class OrderPaymentSyncClient : IOrderPaymentSyncClient
{
    private readonly HttpClient _httpClient;

    public OrderPaymentSyncClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task SyncPaymentAsync(Payment payment, CancellationToken cancellationToken = default)
    {
        var request = MapToOrderPaymentUpdate(payment);
        if (request is null)
        {
            return;
        }

        var response = await _httpClient.PatchAsJsonAsync(
            $"/api/orders/{payment.OrderId}/payment",
            request,
            cancellationToken);

        response.EnsureSuccessStatusCode();
    }

    private static OrderPaymentUpdateRequestDto? MapToOrderPaymentUpdate(Payment payment)
    {
        return payment.Status switch
        {
            PaymentStatus.Pending => new OrderPaymentUpdateRequestDto
            {
                PaymentStatus = "NotPaid",
                PaymentTransactionId = payment.TransactionId,
                PaymentProvider = payment.Provider
            },
            PaymentStatus.Captured => new OrderPaymentUpdateRequestDto
            {
                PaymentStatus = "Paid",
                PaymentTransactionId = payment.TransactionId,
                PaymentProvider = payment.Provider,
                OrderStatus = "Paid"
            },
            PaymentStatus.Failed => new OrderPaymentUpdateRequestDto
            {
                PaymentStatus = "Failed",
                PaymentTransactionId = payment.TransactionId,
                PaymentProvider = payment.Provider
            },
            PaymentStatus.Cancelled => new OrderPaymentUpdateRequestDto
            {
                PaymentStatus = "Cancelled",
                PaymentTransactionId = payment.TransactionId,
                PaymentProvider = payment.Provider,
                OrderStatus = "Cancelled"
            },
            PaymentStatus.Refunded => new OrderPaymentUpdateRequestDto
            {
                PaymentStatus = "Refunded",
                PaymentTransactionId = payment.TransactionId,
                PaymentProvider = payment.Provider,
                OrderStatus = "Refunded"
            },
            _ => null
        };
    }
}
