using System;
using System.Collections.Generic;
using OrderService.Api.Models;

namespace OrderService.Api.DTOs;

public class CreateOrderRequestDto
{
    public Guid CustomerId { get; set; }
    public string Currency { get; set; } = "SEK";

    public CreateAddressDto ShippingAddress { get; set; } = new();
    public CreateAddressDto BillingAddress { get; set; } = new();

    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.CreditCard;
    public string PaymentProvider { get; set; } = string.Empty;
    public string PaymentTransactionId { get; set; } = string.Empty;

    public List<CreateOrderItemDto> Items { get; set; } = [];
}