using CatalogService.Api.Interfaces;
using CatalogService.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace CatalogService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var products = await _productService.GetAllAsync(cancellationToken);
        return Ok(products);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var product = await _productService.GetByIdAsync(id, cancellationToken);
        return product is null ? NotFound() : Ok(product);
    }

    [HttpGet("category/{categoryId:guid}")]
    public async Task<IActionResult> GetByCategory(Guid categoryId, CancellationToken cancellationToken)
    {
        var products = await _productService.GetByCategoryIdAsync(categoryId, cancellationToken);
        return Ok(products);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Product product, CancellationToken cancellationToken)
    {
        var created = await _productService.CreateAsync(product, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] Product product, CancellationToken cancellationToken)
    {
        if (id != product.Id)
        {
            return BadRequest("Id in route does not match product id.");
        }

        var updated = await _productService.UpdateAsync(product, cancellationToken);
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await _productService.DeleteAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }
}