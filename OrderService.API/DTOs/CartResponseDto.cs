using System.Collections.Generic;

namespace OrderService.Api.DTOs;

public class CartResponseDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Currency { get; set; } = "SEK";

    public decimal SubtotalAmount { get; set; }

    public List<CartItemResponseDto> Items { get; set; } = [];
}
