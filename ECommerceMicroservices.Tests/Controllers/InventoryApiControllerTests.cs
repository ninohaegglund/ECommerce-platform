using FluentAssertions;
using InventoryService.Api.Controllers;
using InventoryService.Api.DTOs.Reservations;
using InventoryService.Api.Interfaces;
using InventoryService.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace ECommerceMicroservices.Tests.Controllers;

public class InventoryApiControllerTests
{
    [Fact]
    public async Task GetStock_ReturnsNotFound_WhenProductHasNoStock()
    {
        var productId = Guid.NewGuid();
        var inventoryService = new Mock<IInventoryService>();
        inventoryService
            .Setup(x => x.GetByProductIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((StockItem?)null);

        var controller = new InventoryController(inventoryService.Object);

        var result = await controller.GetStock(productId, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Reserve_ReturnsCreatedAtRoute_WhenStockIsReserved()
    {
        var request = new ReserveStockRequestDto
        {
            ProductId = Guid.NewGuid(),
            OrderId = Guid.NewGuid(),
            Quantity = 2
        };
        var reservation = new StockReservation
        {
            Id = Guid.NewGuid(),
            ProductId = request.ProductId,
            OrderId = request.OrderId,
            Quantity = request.Quantity,
            Status = ReservationStatus.Pending
        };

        var inventoryService = new Mock<IInventoryService>();
        inventoryService
            .Setup(x => x.ReserveAsync(request.ProductId, request.OrderId, request.Quantity, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reservation);

        var controller = new InventoryController(inventoryService.Object);

        var result = await controller.Reserve(request, CancellationToken.None);

        var created = result.Should().BeOfType<CreatedAtRouteResult>().Which;
        created.RouteName.Should().Be("GetReservationById");
        created.RouteValues.Should().ContainKey("reservationId").WhoseValue.Should().Be(reservation.Id);

        var response = created.Value.Should().BeOfType<ReservationResponseDto>().Which;
        response.ProductId.Should().Be(request.ProductId);
        response.Quantity.Should().Be(request.Quantity);
    }

    [Fact]
    public async Task Reserve_ReturnsBadRequest_WhenServiceRejectsReservation()
    {
        var request = new ReserveStockRequestDto
        {
            ProductId = Guid.NewGuid(),
            OrderId = Guid.NewGuid(),
            Quantity = 99
        };

        var inventoryService = new Mock<IInventoryService>();
        inventoryService
            .Setup(x => x.ReserveAsync(request.ProductId, request.OrderId, request.Quantity, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Insufficient stock."));

        var controller = new InventoryController(inventoryService.Object);

        var result = await controller.Reserve(request, CancellationToken.None);

        var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Which;
        badRequest.Value.Should().BeEquivalentTo(new { error = "Insufficient stock." });
    }

    [Fact]
    public async Task Confirm_ReturnsConflict_WhenReservationCannotBeConfirmed()
    {
        var reservationId = Guid.NewGuid();
        var inventoryService = new Mock<IInventoryService>();
        inventoryService
            .Setup(x => x.ConfirmReservationAsync(reservationId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Reservation already released."));

        var controller = new InventoryController(inventoryService.Object);

        var result = await controller.Confirm(reservationId, CancellationToken.None);

        var conflict = result.Should().BeOfType<ConflictObjectResult>().Which;
        conflict.Value.Should().BeEquivalentTo(new { error = "Reservation already released." });
    }
}
