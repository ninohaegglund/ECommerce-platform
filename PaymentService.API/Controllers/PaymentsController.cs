using Microsoft.AspNetCore.Mvc;
using PaymentService.Api.DTOs;
using PaymentService.Api.Interfaces;
using PaymentService.Api.Models;

namespace PaymentService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentsController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
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