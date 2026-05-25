namespace OrderService.Api.Models;

public class WishlistItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid WishlistId { get; set; }

    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public string Currency { get; set; } = "SEK";

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
