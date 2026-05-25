using System.Collections.Generic;

namespace OrderService.Api.Models;

public class Wishlist
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }

    public List<WishlistItem> Items { get; set; } = [];
}
