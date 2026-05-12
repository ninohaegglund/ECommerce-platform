using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using PaymentService.Api.DTOs;
using PaymentService.Api.Interfaces;
using PaymentService.Api.Models;
using Stripe;

namespace PaymentService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private const string StripePaymentIntentSucceeded = "payment_intent.succeeded";
    private const string StripePaymentIntentPaymentFailed = "payment_intent.payment_failed";
    private const string StripePaymentIntentCanceled = "payment_intent.canceled";

    private readonly IPaymentService _paymentService;
    private readonly IConfiguration _configuration;

    public PaymentsController(IPaymentService paymentService, IConfiguration configuration)
    {
        _paymentService = paymentService;
        _configuration = configuration;
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var payment = await _paymentService.GetByIdAsync(id, cancellationToken);
        return payment is null ? NotFound() : Ok(MapToResponse(payment));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePaymentRequestDto request, CancellationToken cancellationToken)
    {
        if (request.Amount <= 0)
        {
            return BadRequest("Amount must be greater than zero.");
        }

        var payment = new Payment
        {
            OrderId = request.OrderId,
            UserId = request.UserId,
            Amount = request.Amount,
            Currency = request.Currency,
            Method = request.Method,
            Provider = request.Provider
        };

        var created = await _paymentService.CreateAsync(payment, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, MapToResponse(created));
    }

    [HttpPost("{id:guid}/process")]
    public async Task<IActionResult> Process(Guid id, [FromBody] ProcessPaymentRequestDto request, CancellationToken cancellationToken)
    {
        try
        {
            var processed = await _paymentService.ProcessAsync(id, request, cancellationToken);
            return processed is null ? NotFound() : Ok(MapToResponse(processed));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("{id:guid}/stripe/payment-intent")]
    public async Task<IActionResult> CreateStripePaymentIntent(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _paymentService.CreateStripePaymentIntentAsync(id, cancellationToken);
            return result is null ? NotFound() : Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (StripeException ex)
        {
            return Problem(
                title: "Stripe payment intent failed.",
                detail: ex.StripeError?.Message ?? ex.Message,
                statusCode: StatusCodes.Status502BadGateway);
        }
    }

    [HttpPost("stripe/webhook")]
    public async Task<IActionResult> StripeWebhook(CancellationToken cancellationToken)
    {
        var webhookSecret = _configuration["Stripe:WebhookSecret"];
        if (string.IsNullOrWhiteSpace(webhookSecret))
        {
            return Problem(
                title: "Stripe webhook secret is not configured.",
                statusCode: StatusCodes.Status500InternalServerError);
        }

        var signatureHeader = Request.Headers["Stripe-Signature"].ToString();
        if (string.IsNullOrWhiteSpace(signatureHeader))
        {
            return BadRequest("Missing Stripe signature header.");
        }

        using var reader = new StreamReader(Request.Body);
        var json = await reader.ReadToEndAsync(cancellationToken);

        Event stripeEvent;
        try
        {
            stripeEvent = EventUtility.ConstructEvent(json, signatureHeader, webhookSecret);
        }
        catch (StripeException)
        {
            return BadRequest("Invalid Stripe webhook signature.");
        }

        if (stripeEvent.Data.Object is not PaymentIntent paymentIntent)
        {
            return Ok();
        }

        var update = stripeEvent.Type switch
        {
            StripePaymentIntentSucceeded => (Status: PaymentStatus.Captured, FailureReason: (string?)null),
            StripePaymentIntentPaymentFailed => (Status: PaymentStatus.Failed, FailureReason: paymentIntent.LastPaymentError?.Message),
            StripePaymentIntentCanceled => (Status: PaymentStatus.Cancelled, FailureReason: "Stripe payment intent was canceled."),
            _ => (Status: (PaymentStatus?)null, FailureReason: (string?)null)
        };

        if (update.Status.HasValue)
        {
            await _paymentService.ApplyStripePaymentIntentStatusAsync(
                paymentIntent.Id,
                update.Status.Value,
                update.FailureReason,
                cancellationToken);
        }

        return Ok();
    }

    private static PaymentResponseDto MapToResponse(Payment payment)
    {
        return new PaymentResponseDto
        {
            Id = payment.Id,
            OrderId = payment.OrderId,
            UserId = payment.UserId,
            Amount = payment.Amount,
            Currency = payment.Currency,
            Method = payment.Method,
            Status = payment.Status,
            Provider = payment.Provider,
            TransactionId = payment.TransactionId,
            FailureReason = payment.FailureReason,
            CreatedAtUtc = payment.CreatedAtUtc,
            ProcessedAtUtc = payment.ProcessedAtUtc,
            UpdatedAtUtc = payment.UpdatedAtUtc
        };
    }
}
