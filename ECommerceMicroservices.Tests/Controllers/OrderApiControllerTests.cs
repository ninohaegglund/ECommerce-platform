using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using OrderService.Api.Controllers;
using OrderService.Api.DTOs;
using OrderService.Api.Interfaces;
using OrderService.Api.Models;

namespace ECommerceMicroservices.Tests.Controllers;

public class OrderApiControllerTests
{
    [Fact]
    public async Task Cart_GetCurrentUserCart_ReturnsUnauthorized_WhenUserIdClaimIsMissing()
    {
        var cartService = new Mock<ICartService>();
        var controller = new CartController(cartService.Object);
        SetAnonymousUser(controller);

        var result = await controller.GetCurrentUserCart(CancellationToken.None);

        var unauthorized = result.Should().BeOfType<UnauthorizedObjectResult>().Which;
        unauthorized.Value.Should().Be("A valid authenticated user account is required.");
        cartService.Verify(x => x.GetCartAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Cart_AddItem_ReturnsBadRequest_WhenQuantityIsNotPositive()
    {
        var cartService = new Mock<ICartService>();
        var controller = new CartController(cartService.Object);
        SetUser(controller, Guid.NewGuid());

        var result = await controller.AddItem(new AddCartItemDto
        {
            ProductId = Guid.NewGuid(),
            Quantity = 0
        }, CancellationToken.None);

        var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Which;
        badRequest.Value.Should().Be("Quantity must be greater than zero.");
        cartService.Verify(x => x.AddItemAsync(
            It.IsAny<Guid>(),
            It.IsAny<AddCartItemDto>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Cart_GetCurrentUserCart_ReturnsCurrentUsersCart()
    {
        var userId = Guid.NewGuid();
        var cart = new CartResponseDto
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            SubtotalAmount = 149
        };

        var cartService = new Mock<ICartService>();
        cartService
            .Setup(x => x.GetCartAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cart);

        var controller = new CartController(cartService.Object);
        SetUser(controller, userId);

        var result = await controller.GetCurrentUserCart(CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Which;
        ok.Value.Should().BeSameAs(cart);
    }

    [Fact]
    public async Task Orders_UpdatePayment_ReturnsBadRequest_WhenPaymentUpdateIsInvalid()
    {
        var orderId = Guid.NewGuid();
        var orderService = new Mock<IOrderService>();
        orderService
            .Setup(x => x.UpdatePaymentAsync(orderId, It.IsAny<UpdateOrderPaymentRequestDto>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Unsupported payment status."));

        var controller = new OrdersController(orderService.Object);

        var result = await controller.UpdatePayment(orderId, new UpdateOrderPaymentRequestDto(), CancellationToken.None);

        var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Which;
        badRequest.Value.Should().Be("Unsupported payment status.");
    }

    [Fact]
    public async Task Wishlist_AddItem_ReturnsNotFound_WhenProductDoesNotExist()
    {
        var userId = Guid.NewGuid();
        var request = new AddWishlistItemDto { ProductId = Guid.NewGuid() };

        var wishlistService = new Mock<IWishlistService>();
        wishlistService
            .Setup(x => x.AddItemAsync(userId, request, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException("Product was not found."));

        var controller = new WishlistController(wishlistService.Object);
        SetUser(controller, userId);

        var result = await controller.AddItem(request, CancellationToken.None);

        var notFound = result.Should().BeOfType<NotFoundObjectResult>().Which;
        notFound.Value.Should().Be("Product was not found.");
    }

    [Fact]
    public async Task Wishlist_AddItem_ReturnsBadRequest_WhenProductIdIsEmpty()
    {
        var wishlistService = new Mock<IWishlistService>();
        var controller = new WishlistController(wishlistService.Object);
        SetUser(controller, Guid.NewGuid());

        var result = await controller.AddItem(new AddWishlistItemDto(), CancellationToken.None);

        var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Which;
        badRequest.Value.Should().Be("ProductId is required.");
        wishlistService.Verify(x => x.AddItemAsync(
            It.IsAny<Guid>(),
            It.IsAny<AddWishlistItemDto>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    private static void SetUser(ControllerBase controller, Guid userId)
    {
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(
                    [new Claim(ClaimTypes.NameIdentifier, userId.ToString())],
                    "TestAuth"))
            }
        };
    }

    private static void SetAnonymousUser(ControllerBase controller)
    {
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity())
            }
        };
    }
}
