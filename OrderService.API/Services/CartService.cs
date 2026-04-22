using OrderService.Api.DTOs;
using OrderService.Api.Interfaces;
using OrderService.Api.Models;
using OrderService.API.DTOs;
using System.Net.Http.Json;

namespace OrderService.Api.Services;

public class CartService : ICartService
{
    private readonly ICartRepository _cartRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IHttpClientFactory _httpClientFactory;

    public CartService(ICartRepository cartRepository, IOrderRepository orderRepository, IHttpClientFactory httpClientFactory)
    {
        _cartRepository = cartRepository;
        _orderRepository = orderRepository;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<CartResponseDto> GetCartAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var cart = await _cartRepository.GetByUserIdAsync(userId, cancellationToken);

        if (cart is null)
        {
            cart = await _cartRepository.AddAsync(new Cart { UserId = userId }, cancellationToken);
        }

        return MapCartResponse(cart);
    }

    public async Task<CartResponseDto> AddItemAsync(Guid userId, AddCartItemDto request, CancellationToken cancellationToken = default)
    {
        var cart = await _cartRepository.GetByUserIdAsync(userId, cancellationToken);

        if (cart is null)
        {
            cart = new Cart
            {
                UserId = userId,
                Currency = string.IsNullOrWhiteSpace(request.Currency)
                    ? "SEK"
                    : request.Currency.ToUpperInvariant()
            };

            cart = await _cartRepository.AddAsync(cart, cancellationToken);
        }

        if (!string.IsNullOrWhiteSpace(request.Currency))
        {
            cart.Currency = request.Currency.ToUpperInvariant();
        }

        var existing = cart.Items.FirstOrDefault(x => x.ProductId == request.ProductId);

        if (existing is null)
        {
            var client = _httpClientFactory.CreateClient("CatalogClient");
            var response = await client.GetAsync($"/api/Products/{request.ProductId}");

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Product not found in Catalog");
            }

            var catalogProduct = await response.Content.ReadFromJsonAsync<CatalogProductDto>();

            var newItem = new CartItem
            {
                CartId = cart.Id,
                ProductId = catalogProduct.Id,
                ProductName = catalogProduct.Name, // Snapshot from Catalog
                Sku = catalogProduct.Sku,          // Snapshot from Catalog
                UnitPrice = catalogProduct.Price,  // Snapshot from Catalog
                Quantity = request.Quantity
            };

            await _cartRepository.AddItemAsync(newItem, cancellationToken);

            cart = await _cartRepository.GetByUserIdAsync(userId, cancellationToken)
                ?? throw new InvalidOperationException("Cart was not found after adding item.");
        }
        else
        {
            var client = _httpClientFactory.CreateClient("CatalogClient");
            var response = await client.GetAsync($"/api/Products/{request.ProductId}");

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Product not found in Catalog");
            }

            var catalogProduct = await response.Content.ReadFromJsonAsync<CatalogProductDto>();

            existing.Quantity += request.Quantity;
            existing.ProductName = catalogProduct.Name;
            existing.Sku = catalogProduct.Sku;
            existing.UnitPrice = catalogProduct.Price;
            cart.UpdatedAtUtc = DateTime.UtcNow;

            await _cartRepository.SaveChangesAsync(cancellationToken);
        }

        return MapCartResponse(cart);
    }

    public async Task<CartResponseDto?> UpdateItemQuantityAsync(Guid userId, Guid itemId, UpdateCartItemQuantityDto request, CancellationToken cancellationToken = default)
    {
        var cart = await _cartRepository.GetByUserIdAsync(userId, cancellationToken);
        if (cart is null)
        {
            return null;
        }

        var item = cart.Items.FirstOrDefault(x => x.Id == itemId);
        if (item is null)
        {
            return null;
        }

        item.Quantity = request.Quantity;
        cart.UpdatedAtUtc = DateTime.UtcNow;

        cart = await _cartRepository.UpdateAsync(cart, cancellationToken);
        return MapCartResponse(cart);
    }

    public async Task<bool> RemoveItemAsync(Guid userId, Guid itemId, CancellationToken cancellationToken = default)
    {
        var cart = await _cartRepository.GetByUserIdAsync(userId, cancellationToken);
        if (cart is null)
        {
            return false;
        }

        var item = cart.Items.FirstOrDefault(x => x.Id == itemId);
        if (item is null)
        {
            return false;
        }

        cart.Items.Remove(item);
        cart.UpdatedAtUtc = DateTime.UtcNow;

        await _cartRepository.UpdateAsync(cart, cancellationToken);
        return true;
    }

    public async Task<Order> CheckoutAsync(Guid userId, CheckoutCartRequestDto request, CancellationToken cancellationToken = default)
    {
        var cart = await _cartRepository.GetByUserIdAsync(userId, cancellationToken);
        if (cart is null || cart.Items.Count == 0)
        {
            throw new InvalidOperationException("Cart is empty.");
        }

        var items = cart.Items.Select(x => new OrderItem
        {
            ProductId = x.ProductId,
            ProductName = x.ProductName,
            Sku = x.Sku,
            Quantity = x.Quantity,
            UnitPrice = x.UnitPrice,
            DiscountAmount = 0m,
            TotalPrice = x.UnitPrice * x.Quantity
        }).ToList();

        var subtotal = items.Sum(x => x.UnitPrice * x.Quantity);
        const decimal shipping = 0m;
        const decimal tax = 0m;
        const decimal discount = 0m;
        var totalAmount = subtotal + shipping + tax - discount;

        var order = new Order
        {
            UserId = userId,
            OrderNumber = $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Random.Shared.Next(1000, 9999)}",
            Status = OrderStatus.Pending,
            PaymentStatus = PaymentStatus.NotPaid,
            PaymentProvider = string.IsNullOrWhiteSpace(request.PaymentProvider) ? null : request.PaymentProvider,
            PaymentTransactionId = string.IsNullOrWhiteSpace(request.PaymentTransactionId) ? null : request.PaymentTransactionId,
            Currency = cart.Currency,
            SubtotalAmount = subtotal,
            ShippingAmount = shipping,
            TaxAmount = tax,
            DiscountAmount = discount,
            TotalAmount = totalAmount < 0 ? 0 : totalAmount,
            ShippingAddress = new Address
            {
                FirstName = request.ShippingAddress.FirstName,
                LastName = request.ShippingAddress.LastName,
                Company = request.ShippingAddress.Company,
                StreetLine1 = request.ShippingAddress.StreetLine1,
                StreetLine2 = request.ShippingAddress.StreetLine2,
                City = request.ShippingAddress.City,
                PostalCode = request.ShippingAddress.PostalCode,
                Region = request.ShippingAddress.Region,
                CountryCode = request.ShippingAddress.CountryCode,
                PhoneNumber = request.ShippingAddress.PhoneNumber
            },
            BillingAddress = new Address
            {
                FirstName = request.BillingAddress.FirstName,
                LastName = request.BillingAddress.LastName,
                Company = request.BillingAddress.Company,
                StreetLine1 = request.BillingAddress.StreetLine1,
                StreetLine2 = request.BillingAddress.StreetLine2,
                City = request.BillingAddress.City,
                PostalCode = request.BillingAddress.PostalCode,
                Region = request.BillingAddress.Region,
                CountryCode = request.BillingAddress.CountryCode,
                PhoneNumber = request.BillingAddress.PhoneNumber
            },
            Items = items
        };

        var createdOrder = await _orderRepository.AddAsync(order, cancellationToken);

        cart.Items.Clear();
        cart.UpdatedAtUtc = DateTime.UtcNow;
        await _cartRepository.UpdateAsync(cart, cancellationToken);

        return createdOrder;
    }

    private static CartResponseDto MapCartResponse(Cart cart)
    {
        var items = cart.Items.Select(x => new CartItemResponseDto
        {
            Id = x.Id,
            ProductId = x.ProductId,
            ProductName = x.ProductName,
            Sku = x.Sku,
            Quantity = x.Quantity,
            UnitPrice = x.UnitPrice,
            LineTotal = x.UnitPrice * x.Quantity
        }).ToList();

        return new CartResponseDto
        {
            Id = cart.Id,
            UserId = cart.UserId,
            Currency = cart.Currency,
            SubtotalAmount = items.Sum(x => x.LineTotal),
            Items = items
        };
    }
}
