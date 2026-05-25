using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderService.Api.DTOs;
using OrderService.Api.Interfaces;
using System.Security.Claims;

namespace OrderService.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/wishlist")]
public class WishlistController : ControllerBase
{
    private readonly IWishlistService _wishlistService;

    public WishlistController(IWishlistService wishlistService)
    {
        _wishlistService = wishlistService;
    }

    [HttpGet]
    public async Task<IActionResult> GetCurrentUserWishlist(CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized("A valid authenticated user account is required.");
        }

        var wishlist = await _wishlistService.GetWishlistAsync(userId, cancellationToken);
        return Ok(wishlist);
    }

    [HttpPost("items")]
    public async Task<IActionResult> AddItem([FromBody] AddWishlistItemDto request, CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized("A valid authenticated user account is required.");
        }

        if (request.ProductId == Guid.Empty)
        {
            return BadRequest("ProductId is required.");
        }

        try
        {
            var wishlist = await _wishlistService.AddItemAsync(userId, request, cancellationToken);
            return Ok(wishlist);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpDelete("items/{itemId:guid}")]
    public async Task<IActionResult> RemoveItem(Guid itemId, CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized("A valid authenticated user account is required.");
        }

        var removed = await _wishlistService.RemoveItemAsync(userId, itemId, cancellationToken);
        return removed ? NoContent() : NotFound();
    }

    private bool TryGetUserId(out Guid userId)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(userIdClaim, out userId);
    }
}
