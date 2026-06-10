using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using PaymentService.Api.Controllers;
using PaymentService.Api.DTOs;
using PaymentService.Api.Interfaces;
using PaymentService.Api.Models;

namespace ECommerceMicroservices.Tests.Controllers;

public class PaymentApiControllerTests
{
    [Fact]
    public async Task Create_ReturnsBadRequest_WhenAmountIsNotPositive()
    {
        var paymentService = new Mock<IPaymentService>();
        var controller = new PaymentsController(paymentService.Object, EmptyConfiguration());

        var result = await controller.Create(new CreatePaymentRequestDto { Amount = 0 }, CancellationToken.None);

        var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Which;
        badRequest.Value.Should().Be("Amount must be greater than zero.");
        paymentService.Verify(x => x.CreateAsync(It.IsAny<Payment>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Create_MapsRequestAndReturnsCreatedPayment()
    {
        var request = new CreatePaymentRequestDto
        {
            OrderId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            OrderNumber = "ORD-1001",
            RecipientEmail = "customer@test.local",
            Amount = 249.50m,
            Currency = "SEK",
            Method = PaymentMethod.Card,
            Provider = "stripe"
        };
        var createdPaymentId = Guid.NewGuid();

        var paymentService = new Mock<IPaymentService>();
        paymentService
            .Setup(x => x.CreateAsync(It.IsAny<Payment>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Payment payment, CancellationToken _) =>
            {
                payment.Id = createdPaymentId;
                return payment;
            });

        var controller = new PaymentsController(paymentService.Object, EmptyConfiguration());

        var result = await controller.Create(request, CancellationToken.None);

        var created = result.Should().BeOfType<CreatedAtActionResult>().Which;
        created.ActionName.Should().Be(nameof(PaymentsController.GetById));
        created.RouteValues.Should().ContainKey("id").WhoseValue.Should().Be(createdPaymentId);

        var response = created.Value.Should().BeOfType<PaymentResponseDto>().Which;
        response.Id.Should().Be(createdPaymentId);
        response.OrderId.Should().Be(request.OrderId);
        response.Amount.Should().Be(request.Amount);
        response.Provider.Should().Be(request.Provider);
    }

    [Fact]
    public async Task Process_ReturnsBadRequest_WhenPaymentCannotBeProcessed()
    {
        var paymentId = Guid.NewGuid();
        var paymentService = new Mock<IPaymentService>();
        paymentService
            .Setup(x => x.ProcessAsync(paymentId, It.IsAny<ProcessPaymentRequestDto>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Payment is already processed."));

        var controller = new PaymentsController(paymentService.Object, EmptyConfiguration());

        var result = await controller.Process(paymentId, new ProcessPaymentRequestDto(), CancellationToken.None);

        var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Which;
        badRequest.Value.Should().Be("Payment is already processed.");
    }

    [Fact]
    public async Task CreateStripePaymentIntent_ReturnsNotFound_WhenPaymentDoesNotExist()
    {
        var paymentId = Guid.NewGuid();
        var paymentService = new Mock<IPaymentService>();
        paymentService
            .Setup(x => x.CreateStripePaymentIntentAsync(paymentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((StripePaymentIntentResponseDto?)null);

        var controller = new PaymentsController(paymentService.Object, EmptyConfiguration());

        var result = await controller.CreateStripePaymentIntent(paymentId, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    private static IConfiguration EmptyConfiguration()
    {
        return new ConfigurationBuilder().Build();
    }
}
