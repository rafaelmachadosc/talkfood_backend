using Backend.Application.DTOs.Product;
using Backend.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Backend.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Route("api/products")] // Rota alternativa para compatibilidade com frontend
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
            // Suporta category_id como Guid ou string (vindo do JSON como string)
            Guid categoryId = Guid.Empty;
            
            if (request.category_id != null)
            {
                // Converter qualquer tipo para string e depois para Guid
                string categoryIdStr = request.category_id switch
                {
                    Guid g => g.ToString(),
                    string s => s,
                    System.Text.Json.JsonElement jsonElement => jsonElement.GetString() ?? string.Empty,
                    _ => request.category_id.ToString() ?? string.Empty
                };
                
                if (string.IsNullOrWhiteSpace(categoryIdStr))
                {
                    return BadRequest(new { error = "Category ID não pode ser vazio" });
                }
                
                if (!Guid.TryParse(categoryIdStr, out categoryId))
                {
                    return BadRequest(new { error = $"Category ID inválido: '{categoryIdStr}'. Deve ser um GUID válido." });
                }
            }
            else
            {
                return BadRequest(new { error = "Category ID é obrigatório" });
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
    [HttpGet("products")] // Suporta também /api/product/products
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
    [System.Text.Json.Serialization.JsonIgnore]
    public Guid CategoryId { get; set; } // Ignorado no JSON
    [System.Text.Json.Serialization.JsonIgnore]
    public Guid? categoryId { get; set; } // Ignorado no JSON
    [System.Text.Json.Serialization.JsonPropertyName("category_id")]
    public object? category_id { get; set; } // snake_case do frontend - pode ser Guid ou string
}

public class UpdateProductRequestDto
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public int? Price { get; set; }
    public string? Description { get; set; }
    public bool? Disabled { get; set; }
}
