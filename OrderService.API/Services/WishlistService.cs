using OrderService.Api.DTOs;
using OrderService.Api.Interfaces;
using OrderService.Api.Models;
using OrderService.API.DTOs;
using System.Net.Http.Json;

namespace OrderService.Api.Services;

public class WishlistService : IWishlistService
{
    private readonly IWishlistRepository _wishlistRepository;
    private readonly IHttpClientFactory _httpClientFactory;

    public WishlistService(IWishlistRepository wishlistRepository, IHttpClientFactory httpClientFactory)
    {
        _wishlistRepository = wishlistRepository;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<WishlistResponseDto> GetWishlistAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var wishlist = await _wishlistRepository.GetByUserIdAsync(userId, cancellationToken);

        if (wishlist is null)
        {
            wishlist = await _wishlistRepository.AddAsync(new Wishlist { UserId = userId }, cancellationToken);
        }

        return MapWishlistResponse(wishlist);
    }

    public async Task<WishlistResponseDto> AddItemAsync(Guid userId, AddWishlistItemDto request, CancellationToken cancellationToken = default)
    {
        var wishlist = await _wishlistRepository.GetByUserIdAsync(userId, cancellationToken);

        if (wishlist is null)
        {
            wishlist = await _wishlistRepository.AddAsync(new Wishlist { UserId = userId }, cancellationToken);
        }

        var catalogProduct = await GetCatalogProductAsync(request.ProductId, cancellationToken);
        var existing = wishlist.Items.FirstOrDefault(x => x.ProductId == request.ProductId);

        if (existing is null)
        {
            var newItem = new WishlistItem
            {
                WishlistId = wishlist.Id,
                ProductId = catalogProduct.Id,
                ProductName = catalogProduct.Name,
                Sku = catalogProduct.Sku,
                UnitPrice = catalogProduct.Price,
                Currency = string.IsNullOrWhiteSpace(catalogProduct.Currency)
                    ? "SEK"
                    : catalogProduct.Currency.ToUpperInvariant()
            };

            wishlist.UpdatedAtUtc = DateTime.UtcNow;
            await _wishlistRepository.AddItemAsync(newItem, cancellationToken);

            wishlist = await _wishlistRepository.GetByUserIdAsync(userId, cancellationToken)
                ?? throw new InvalidOperationException("Wishlist was not found after adding item.");
        }
        else
        {
            existing.ProductName = catalogProduct.Name;
            existing.Sku = catalogProduct.Sku;
            existing.UnitPrice = catalogProduct.Price;
            existing.Currency = string.IsNullOrWhiteSpace(catalogProduct.Currency)
                ? existing.Currency
                : catalogProduct.Currency.ToUpperInvariant();
            wishlist.UpdatedAtUtc = DateTime.UtcNow;

            await _wishlistRepository.SaveChangesAsync(cancellationToken);
        }

        return MapWishlistResponse(wishlist);
    }

    public async Task<bool> RemoveItemAsync(Guid userId, Guid itemId, CancellationToken cancellationToken = default)
    {
        var wishlist = await _wishlistRepository.GetByUserIdAsync(userId, cancellationToken);
        if (wishlist is null)
        {
            return false;
        }

        var item = wishlist.Items.FirstOrDefault(x => x.Id == itemId);
        if (item is null)
        {
            return false;
        }

        wishlist.Items.Remove(item);
        wishlist.UpdatedAtUtc = DateTime.UtcNow;

        await _wishlistRepository.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task<CatalogProductDto> GetCatalogProductAsync(Guid productId, CancellationToken cancellationToken)
    {
        var client = _httpClientFactory.CreateClient("CatalogClient");
        var response = await client.GetAsync($"/api/Products/{productId}", cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new KeyNotFoundException("Product not found in Catalog.");
        }

        return await response.Content.ReadFromJsonAsync<CatalogProductDto>(cancellationToken)
            ?? throw new InvalidOperationException("Catalog returned an empty product response.");
    }

    private static WishlistResponseDto MapWishlistResponse(Wishlist wishlist)
    {
        return new WishlistResponseDto
        {
            Id = wishlist.Id,
            UserId = wishlist.UserId,
            Items = wishlist.Items
                .OrderByDescending(x => x.CreatedAtUtc)
                .Select(x => new WishlistItemResponseDto
                {
                    Id = x.Id,
                    ProductId = x.ProductId,
                    ProductName = x.ProductName,
                    Sku = x.Sku,
                    UnitPrice = x.UnitPrice,
                    Currency = x.Currency,
                    CreatedAtUtc = x.CreatedAtUtc
                })
                .ToList()
        };
    }
}
