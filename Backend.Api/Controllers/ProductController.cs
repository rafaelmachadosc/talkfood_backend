using Backend.Application.DTOs.Product;
using Backend.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductController : ControllerBase
{
    private readonly ProductService _productService;

    public ProductController(ProductService productService)
    {
        _productService = productService;
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ProductDto>> CreateProduct([FromBody] CreateProductRequestDto request, CancellationToken cancellationToken)
    {
        try
        {
            // Suporta CategoryId em PascalCase, camelCase ou snake_case
            Guid categoryId;
            if (request.CategoryId != Guid.Empty)
            {
                categoryId = request.CategoryId;
            }
            else if (request.categoryId.HasValue)
            {
                categoryId = request.categoryId.Value;
            }
            else if (request.category_id.HasValue)
            {
                categoryId = request.category_id.Value;
            }
            else
            {
                throw new ArgumentException("Category ID é obrigatório");
            }

            var product = await _productService.CreateProductAsync(
                request.Name,
                request.Price,
                request.Description,
                categoryId,
                cancellationToken
            );
            return Ok(product);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet]
    [Authorize]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetAllProducts([FromQuery] bool? disabled, CancellationToken cancellationToken)
    {
        var products = await _productService.GetAllProductsAsync(disabled, cancellationToken);
        return Ok(products);
    }

    [HttpGet("public")]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetAllProductsPublic([FromQuery] bool? disabled, CancellationToken cancellationToken)
    {
        var products = await _productService.GetAllProductsAsync(disabled ?? false, cancellationToken);
        return Ok(products);
    }

    [HttpGet("category/{categoryId}")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetProductsByCategory(Guid categoryId, CancellationToken cancellationToken)
    {
        var products = await _productService.GetProductsByCategoryAsync(categoryId, cancellationToken);
        return Ok(products);
    }

    [HttpPut]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ProductDto>> UpdateProduct([FromBody] UpdateProductRequestDto request, CancellationToken cancellationToken)
    {
        try
        {
            var product = await _productService.UpdateProductAsync(
                request.Id,
                request.Name,
                request.Price,
                request.Description,
                request.Disabled,
                cancellationToken
            );
            return Ok(product);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> DeleteProduct(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            await _productService.DeleteProductAsync(id, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }
}

public class CreateProductRequestDto
{
    public string Name { get; set; } = string.Empty;
    public int Price { get; set; }
    public string Description { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public Guid? categoryId { get; set; } // camelCase do frontend
    public Guid? category_id { get; set; } // snake_case do frontend
}

public class UpdateProductRequestDto
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public int? Price { get; set; }
    public string? Description { get; set; }
    public bool? Disabled { get; set; }
}
