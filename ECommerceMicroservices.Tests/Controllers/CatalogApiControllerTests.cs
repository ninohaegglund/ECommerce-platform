using CatalogService.Api.Controllers;
using CatalogService.Api.DTOs.Products;
using CatalogService.Api.Interfaces;
using CatalogService.Api.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace ECommerceMicroservices.Tests.Controllers;

public class CatalogApiControllerTests
{
    [Fact]
    public async Task Products_GetById_ReturnsNotFound_WhenProductDoesNotExist()
    {
        var productService = new Mock<IProductService>();
        var productId = Guid.NewGuid();
        productService
            .Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        var controller = new ProductsController(productService.Object);

        var result = await controller.GetById(productId, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Products_GetImages_ReturnsImagesOrderedBySortOrder()
    {
        var firstImageId = Guid.NewGuid();
        var secondImageId = Guid.NewGuid();
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Images =
            [
                new ProductImage { Id = firstImageId, ImageUrl = "https://cdn.test/second.jpg", SortOrder = 20 },
                new ProductImage { Id = secondImageId, ImageUrl = "https://cdn.test/first.jpg", SortOrder = 10 }
            ]
        };

        var productService = new Mock<IProductService>();
        productService
            .Setup(x => x.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        var controller = new ProductsController(productService.Object);

        var result = await controller.GetImages(product.Id, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Which;
        var images = Assert.IsAssignableFrom<IEnumerable<ProductImageResponseDto>>(ok.Value);
        images.Select(x => x.Id).Should().Equal(secondImageId, firstImageId);
    }

    [Fact]
    public async Task Products_Create_ReturnsBadRequest_WhenCategoryIsMissing()
    {
        var request = new CreateProductRequestDto
        {
            CategoryId = Guid.NewGuid(),
            Name = "Keyboard",
            Slug = "keyboard",
            Sku = "SKU-KEYBOARD",
            ShortDescription = "Mechanical keyboard",
            Description = "Mechanical keyboard with nordic layout",
            Price = 999,
            Currency = "SEK"
        };

        var productService = new Mock<IProductService>();
        productService
            .Setup(x => x.CreateAsync(It.Is<Product>(product =>
                product.CategoryId == request.CategoryId &&
                product.Name == request.Name &&
                product.Sku == request.Sku), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        var controller = new ProductsController(productService.Object);

        var result = await controller.Create(request, CancellationToken.None);

        var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Which;
        badRequest.Value.Should().Be($"Category '{request.CategoryId}' does not exist.");
    }

    [Fact]
    public async Task Categories_Delete_ReturnsNoContent_WhenDeleted()
    {
        var categoryId = Guid.NewGuid();
        var categoryService = new Mock<ICategoryService>();
        categoryService
            .Setup(x => x.DeleteAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var controller = new CategoriesController(categoryService.Object);

        var result = await controller.Delete(categoryId, CancellationToken.None);

        result.Should().BeOfType<NoContentResult>();
    }
}
