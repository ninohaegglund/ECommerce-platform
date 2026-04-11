using System.Collections.Generic;

namespace OrderService.Api.Models;

public class Cart
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }

    public string Currency { get; set; } = "SEK";

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }

    public List<CartItem> Items { get; set; } = [];
}
