using System;
using System.Collections.Generic;

namespace OrderService.Api.Models;

public class Order
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CustomerId { get; set; }

    public string OrderNumber { get; set; } = string.Empty;
    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    public decimal SubtotalAmount { get; set; }
    public decimal ShippingAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }

    public string Currency { get; set; } = "SEK";

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }

    public Address ShippingAddress { get; set; } = new();
    public Address BillingAddress { get; set; } = new();

    public Payment Payment { get; set; } = new();

    public List<OrderItem> Items { get; set; } = [];
}