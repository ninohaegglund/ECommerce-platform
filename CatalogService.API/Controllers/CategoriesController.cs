using CatalogService.Api.DTOs.Categories;
using CatalogService.Api.Interfaces;
using CatalogService.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace CatalogService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var categories = await _categoryService.GetAllAsync(cancellationToken);
        return Ok(categories.Select(MapToResponse));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var category = await _categoryService.GetByIdAsync(id, cancellationToken);
        return category is null ? NotFound() : Ok(MapToResponse(category));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCategoryRequestDto request, CancellationToken cancellationToken)
    {
        var category = new Category
        {
            Name = request.Name,
            Slug = request.Slug,
            Description = request.Description,
            IsActive = request.IsActive,
            ParentCategoryId = request.ParentCategoryId
        };

        var created = await _categoryService.CreateAsync(category, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, MapToResponse(created));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCategoryRequestDto request, CancellationToken cancellationToken)
    {
        var category = new Category
        {
            Id = id,
            Name = request.Name,
            Slug = request.Slug,
            Description = request.Description,
            IsActive = request.IsActive,
            ParentCategoryId = request.ParentCategoryId
        };

        var updated = await _categoryService.UpdateAsync(category, cancellationToken);
        return updated is null ? NotFound() : Ok(MapToResponse(updated));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await _categoryService.DeleteAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }

    private static CategoryResponseDto MapToResponse(Category category)
    {
        return new CategoryResponseDto
        {
            Id = category.Id,
            Name = category.Name,
            Slug = category.Slug,
            Description = category.Description,
            IsActive = category.IsActive,
            ParentCategoryId = category.ParentCategoryId,
            ChildCategoryIds = category.Children.Select(x => x.Id).ToList(),
            ProductCount = category.Products.Count,
            CreatedAtUtc = category.CreatedAtUtc,
            UpdatedAtUtc = category.UpdatedAtUtc
        };
    }
}   