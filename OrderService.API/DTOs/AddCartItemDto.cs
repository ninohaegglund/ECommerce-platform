namespace OrderService.Api.DTOs;

public class AddCartItemDto
{
    public Guid ProductId { get; set; }

    public int Quantity { get; set; }

    public string Currency { get; set; } = "SEK";
}
