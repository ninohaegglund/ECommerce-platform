using InventoryService.Api.DTOs.Reservations;
using InventoryService.Api.DTOs.StockItems;
using InventoryService.Api.Interfaces;
using InventoryService.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace InventoryService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InventoryController : ControllerBase
{
    private readonly IInventoryService _inventoryService;

    public InventoryController(IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    [HttpGet("{productId:guid}")]
    public async Task<IActionResult> GetStock(Guid productId, CancellationToken cancellationToken)
    {
        var stock = await _inventoryService.GetByProductIdAsync(productId, cancellationToken);
        return stock is null ? NotFound() : Ok(MapToResponse(stock));
    }

    [HttpGet("reservations/{reservationId:guid}", Name = "GetReservationById")]
    public async Task<IActionResult> GetReservation(Guid reservationId, CancellationToken cancellationToken)
    {
        var reservation = await _inventoryService.GetReservationByIdAsync(reservationId, cancellationToken);
        return reservation is null ? NotFound() : Ok(MapToResponse(reservation));
    }

    [HttpPost("reserve")]
    public async Task<IActionResult> Reserve([FromBody] ReserveStockRequestDto request, CancellationToken cancellationToken)
    {
        try
        {
            var reservation = await _inventoryService.ReserveAsync(
                request.ProductId, request.OrderId, request.Quantity, cancellationToken);
            return CreatedAtRoute(
                "GetReservationById",
                new { reservationId = reservation.Id },
                MapToResponse(reservation));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("confirm/{reservationId:guid}")]
    public async Task<IActionResult> Confirm(Guid reservationId, CancellationToken cancellationToken)
    {
        try
        {
            var reservation = await _inventoryService.ConfirmReservationAsync(reservationId, cancellationToken);
            return reservation is null ? NotFound() : Ok(MapToResponse(reservation));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
    }

    [HttpPost("release/{reservationId:guid}")]
    public async Task<IActionResult> Release(Guid reservationId, CancellationToken cancellationToken)
    {
        try
        {
            var reservation = await _inventoryService.ReleaseReservationAsync(reservationId, cancellationToken);
            return reservation is null ? NotFound() : Ok(MapToResponse(reservation));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("stock")]
    public async Task<IActionResult> SetStock([FromBody] SetStockRequestDto request, CancellationToken cancellationToken)
    {
        try
        {
            var stock = await _inventoryService.SetStockAsync(
                request.ProductId, request.QuantityAvailable, cancellationToken);
            return Ok(MapToResponse(stock));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    private static StockItemResponseDto MapToResponse(StockItem stock)
    {
        return new StockItemResponseDto
        {
            Id = stock.Id,
            ProductId = stock.ProductId,
            QuantityAvailable = stock.QuantityAvailable,
            QuantityReserved = stock.QuantityReserved,
            CreatedAtUtc = stock.CreatedAtUtc,
            UpdatedAtUtc = stock.UpdatedAtUtc
        };
    }

    private static ReservationResponseDto MapToResponse(StockReservation reservation)
    {
        return new ReservationResponseDto
        {
            Id = reservation.Id,
            ProductId = reservation.ProductId,
            OrderId = reservation.OrderId,
            Quantity = reservation.Quantity,
            Status = reservation.Status,
            CreatedAtUtc = reservation.CreatedAtUtc,
            UpdatedAtUtc = reservation.UpdatedAtUtc
        };
    }
}
