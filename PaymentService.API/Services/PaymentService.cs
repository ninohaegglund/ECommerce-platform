using PaymentService.Api.DTOs;
using PaymentService.Api.Interfaces;
using PaymentService.Api.Models;
using Microsoft.Extensions.Configuration;
using Stripe;
using PaymentMethodModel = PaymentService.Api.Models.PaymentMethod;

namespace PaymentService.Api.Services;

public class PaymentService : IPaymentService
{
    private const string StripeProvider = "Stripe";

    private static readonly HashSet<string> ZeroDecimalCurrencies = new(StringComparer.OrdinalIgnoreCase)
    {
        "bif", "clp", "djf", "gnf", "jpy", "kmf", "krw", "mga", "pyg", "rwf", "ugx", "vnd", "vuv", "xaf", "xof", "xpf"
    };

    private readonly IPaymentRepository _paymentRepository;
    private readonly IOrderPaymentSyncClient _orderPaymentSyncClient;
    private readonly IConfiguration _configuration;
    private readonly INotificationClient _notificationClient;

    public PaymentService(
        IPaymentRepository paymentRepository,
        IConfiguration configuration,
        INotificationClient notificationClient)
        IOrderPaymentSyncClient orderPaymentSyncClient,
        IConfiguration configuration)
    {
        _paymentRepository = paymentRepository;
        _orderPaymentSyncClient = orderPaymentSyncClient;
        _configuration = configuration;
        _notificationClient = notificationClient;
    }

    public Task<Payment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _paymentRepository.GetByIdAsync(id, cancellationToken);
    }

    public Task<Payment?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        return _paymentRepository.GetByOrderIdAsync(orderId, cancellationToken);
    }

    public async Task<Payment> CreateAsync(Payment payment, CancellationToken cancellationToken = default)
    {
        payment.Status = PaymentStatus.Pending;
        payment.CreatedAtUtc = DateTime.UtcNow;
        payment.UpdatedAtUtc = null;
        payment.ProcessedAtUtc = null;

        return await _paymentRepository.AddAsync(payment, cancellationToken);
    }

    public async Task<Payment?> ProcessAsync(Guid id, ProcessPaymentRequestDto request, CancellationToken cancellationToken = default)
    {
        var payment = await _paymentRepository.GetByIdAsync(id, cancellationToken);
        if (payment is null)
        {
            return null;
        }

        if (payment.Status is PaymentStatus.Captured or PaymentStatus.Failed or PaymentStatus.Cancelled)
        {
            throw new InvalidOperationException("Payment is already finalized.");
        }

        payment.Status = request.IsSuccessful ? PaymentStatus.Captured : PaymentStatus.Failed;
        payment.TransactionId = request.TransactionId;
        payment.FailureReason = request.IsSuccessful ? null : request.FailureReason;
        var now = DateTime.UtcNow;

        payment.ProcessedAtUtc = now;
        payment.UpdatedAtUtc = now;

        var updatedPayment = await _paymentRepository.UpdateAsync(payment, cancellationToken);

        if (updatedPayment.Status == PaymentStatus.Captured)
        {
            await SendPaymentConfirmationAsync(updatedPayment, cancellationToken);
        }

        return updatedPayment;
        var updated = await _paymentRepository.UpdateAsync(payment, cancellationToken);
        await _orderPaymentSyncClient.SyncPaymentAsync(updated, cancellationToken);

        return updated;
    }

    public async Task<StripePaymentIntentResponseDto?> CreateStripePaymentIntentAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var payment = await _paymentRepository.GetByIdAsync(id, cancellationToken);
        if (payment is null)
        {
            return null;
        }

        if (payment.Method != PaymentMethodModel.Card)
        {
            throw new InvalidOperationException("Stripe payments currently support card payments only.");
        }

        if (payment.Status is PaymentStatus.Captured or PaymentStatus.Failed or PaymentStatus.Cancelled)
        {
            throw new InvalidOperationException("Payment is already finalized.");
        }

        if (!string.IsNullOrWhiteSpace(payment.Provider) &&
            !string.Equals(payment.Provider, StripeProvider, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"Payment provider is already set to {payment.Provider}.");
        }

        var paymentIntentService = new PaymentIntentService(CreateStripeClient());
        PaymentIntent paymentIntent;

        if (!string.IsNullOrWhiteSpace(payment.TransactionId))
        {
            paymentIntent = await paymentIntentService.GetAsync(payment.TransactionId, cancellationToken: cancellationToken);
        }
        else
        {
            var createOptions = new PaymentIntentCreateOptions
            {
                Amount = ToStripeAmount(payment.Amount, payment.Currency),
                Currency = NormalizeCurrency(payment.Currency),
                PaymentMethodTypes = new List<string> { "card" },
                Description = $"Order {payment.OrderId}",
                Metadata = new Dictionary<string, string>
                {
                    ["paymentId"] = payment.Id.ToString(),
                    ["orderId"] = payment.OrderId.ToString(),
                    ["userId"] = payment.UserId.ToString()
                }
            };

            var requestOptions = new RequestOptions
            {
                IdempotencyKey = $"payment-intent-{payment.Id}"
            };

            paymentIntent = await paymentIntentService.CreateAsync(createOptions, requestOptions, cancellationToken);
        }

        if (string.IsNullOrWhiteSpace(paymentIntent.ClientSecret))
        {
            throw new InvalidOperationException("Stripe did not return a client secret for the payment intent.");
        }

        payment.Provider = StripeProvider;
        payment.TransactionId = paymentIntent.Id;
        payment.Status = PaymentStatus.Pending;
        payment.FailureReason = null;
        payment.UpdatedAtUtc = DateTime.UtcNow;

        await _paymentRepository.UpdateAsync(payment, cancellationToken);

        return new StripePaymentIntentResponseDto
        {
            PaymentId = payment.Id,
            PaymentIntentId = paymentIntent.Id,
            ClientSecret = paymentIntent.ClientSecret,
            Status = payment.Status
        };
    }

    public async Task<Payment?> ApplyStripePaymentIntentStatusAsync(
        string paymentIntentId,
        PaymentStatus status,
        string? failureReason,
        CancellationToken cancellationToken = default)
    {
        var payment = await _paymentRepository.GetByTransactionIdAsync(paymentIntentId, cancellationToken);
        if (payment is null)
        {
            return null;
        }

        var wasCaptured = payment.Status == PaymentStatus.Captured;
        var now = DateTime.UtcNow;

        payment.Status = status;
        payment.FailureReason = failureReason;
        payment.UpdatedAtUtc = now;

        if (status is PaymentStatus.Captured or PaymentStatus.Failed or PaymentStatus.Cancelled or PaymentStatus.Refunded)
        {
            payment.ProcessedAtUtc = now;
        }

        var updatedPayment = await _paymentRepository.UpdateAsync(payment, cancellationToken);

        if (!wasCaptured && updatedPayment.Status == PaymentStatus.Captured)
        {
            await SendPaymentConfirmationAsync(updatedPayment, cancellationToken);
        }

        return updatedPayment;
        var updated = await _paymentRepository.UpdateAsync(payment, cancellationToken);
        await _orderPaymentSyncClient.SyncPaymentAsync(updated, cancellationToken);

        return updated;
    }

    private async Task SendPaymentConfirmationAsync(Payment payment, CancellationToken cancellationToken)
    {
        await _notificationClient.SendPaymentConfirmationAsync(
            new PaymentConfirmationNotificationRequest(
                payment.UserId,
                payment.OrderId,
                GetOrderNumber(payment),
                payment.RecipientEmail,
                payment.Amount,
                payment.Currency,
                GetTransactionId(payment)),
            cancellationToken);
    }

    private static string GetOrderNumber(Payment payment)
        => string.IsNullOrWhiteSpace(payment.OrderNumber) ? payment.OrderId.ToString() : payment.OrderNumber;

    private static string GetTransactionId(Payment payment)
        => string.IsNullOrWhiteSpace(payment.TransactionId) ? payment.Id.ToString() : payment.TransactionId;

    private StripeClient CreateStripeClient()
    {
        var secretKey = _configuration["Stripe:SecretKey"];
        if (string.IsNullOrWhiteSpace(secretKey))
        {
            throw new InvalidOperationException("Stripe:SecretKey is not configured.");
        }

        return new StripeClient(secretKey);
    }

    private static long ToStripeAmount(decimal amount, string currency)
    {
        if (amount <= 0)
        {
            throw new InvalidOperationException("Amount must be greater than zero.");
        }

        var normalizedCurrency = NormalizeCurrency(currency);
        var multiplier = ZeroDecimalCurrencies.Contains(normalizedCurrency) ? 1 : 100;
        var amountInSmallestUnit = amount * multiplier;

        if (amountInSmallestUnit != decimal.Truncate(amountInSmallestUnit))
        {
            throw new InvalidOperationException("Amount contains too many decimal places for the selected currency.");
        }

        return checked((long)amountInSmallestUnit);
    }

    private static string NormalizeCurrency(string currency)
    {
        var normalizedCurrency = currency.Trim().ToLowerInvariant();
        if (normalizedCurrency.Length != 3)
        {
            throw new InvalidOperationException("Currency must be a three-letter ISO currency code.");
        }

        return normalizedCurrency;
    }
}
