namespace PaymentService.Api.DTOs;

public class ProcessPaymentRequestDto
{
    public bool IsSuccessful { get; set; }
    public string? TransactionId { get; set; }
    public string? FailureReason { get; set; }
}