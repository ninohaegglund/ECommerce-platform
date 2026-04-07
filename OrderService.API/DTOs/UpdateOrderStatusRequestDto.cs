using OrderService.Api.Models;

namespace OrderService.Api.DTOs;

public class UpdateOrderStatusRequestDto
{
    public OrderStatus Status { get; set; }
}