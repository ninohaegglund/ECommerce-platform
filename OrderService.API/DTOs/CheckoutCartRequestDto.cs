namespace OrderService.Api.DTOs;

public class CheckoutCartRequestDto
{
    public CreateAddressDto ShippingAddress { get; set; } = new();
    public CreateAddressDto BillingAddress { get; set; } = new();

    public string? PaymentProvider { get; set; }
    public string? PaymentTransactionId { get; set; }
}
