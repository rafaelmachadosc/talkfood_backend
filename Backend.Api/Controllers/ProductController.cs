using Backend.Application.DTOs.Product;
using Backend.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Backend.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Route("api/products")] // Rota alternativa para compatibilidade com frontend
public class ProductController : ControllerBase
{
    private readonly ProductService _productService;
    private readonly ILogger<ProductController> _logger;

    public ProductController(ProductService productService, ILogger<ProductController> logger)
    {
        _productService = productService;
        _logger = logger;
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ProductDto>> CreateProduct([FromBody] CreateProductRequestDto request, CancellationToken cancellationToken)
    {
        try
        {
            // Validações básicas
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return BadRequest(new { error = "Nome do produto é obrigatório" });
            }

            if (request.Price < 0)
            {
                return BadRequest(new { error = "Preço não pode ser negativo" });
            }

            var product = await _productService.CreateProductAsync(
                request.Name.Trim(),
                request.Price,
                request.Description ?? string.Empty,
                request.Category ?? string.Empty,
                cancellationToken
            );
            
            return Ok(product);
        }
        catch (DbUpdateException dbEx)
        {
            // Erro específico do banco de dados
            _logger.LogError(dbEx, "Erro ao salvar produto no banco de dados: {Message}", dbEx.Message);
            return StatusCode(500, new { 
                error = "Erro ao salvar no banco de dados", 
                details = dbEx.InnerException?.Message ?? dbEx.Message 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar produto: {Message}", ex.Message);
            return StatusCode(500, new { 
                error = "Erro ao criar produto", 
                details = ex.Message
            });
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
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetAllProductsPublic([FromQuery] bool? disabled, CancellationToken cancellationToken)
    {
        var products = await _productService.GetAllProductsAsync(disabled ?? false, cancellationToken);
        return Ok(products);
    }

    [HttpGet("search")]
    [Authorize]
    public async Task<ActionResult<SearchProductsResponseDto>> SearchProducts([FromQuery] string q, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
        {
            return BadRequest(new { error = "Termo de busca deve ter pelo menos 2 caracteres" });
        }

        var products = await _productService.SearchProductsAsync(q, cancellationToken);
        return Ok(new SearchProductsResponseDto
        {
            products = products.Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                Description = p.Description,
                Disabled = p.Disabled,
                Category = p.Category,
                CreatedAt = p.CreatedAt
            }).ToList()
        });
    }

    [HttpGet("category/{category}")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetProductsByCategory(string category, CancellationToken cancellationToken)
    {
        var products = await _productService.GetProductsByCategoryAsync(category, cancellationToken);
        return Ok(products);
    }

    [HttpPut]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ProductDto>> UpdateProduct([FromBody] UpdateProductRequestDto request, CancellationToken cancellationToken)
    {
        try
        {
            Guid productId;
            if (request.Id != Guid.Empty)
            {
                productId = request.Id;
            }
            else if (!string.IsNullOrEmpty(request.product_id))
            {
                if (!Guid.TryParse(request.product_id, out productId))
                {
                    return BadRequest(new { error = "Product ID inválido" });
                }
            }
            else
            {
                return BadRequest(new { error = "Product ID é obrigatório" });
            }

            var product = await _productService.UpdateProductAsync(
                productId,
                request.Name,
                request.Price,
                request.Description,
                request.Disabled,
                request.Category,
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
    [HttpDelete] // Suporta também /api/product?product_id={id}
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> DeleteProduct(Guid? id, [FromQuery] string? product_id, CancellationToken cancellationToken)
    {
        try
        {
            Guid productId;
            if (id.HasValue)
            {
                productId = id.Value;
            }
            else if (!string.IsNullOrEmpty(product_id))
            {
                if (!Guid.TryParse(product_id, out productId))
                {
                    return BadRequest(new { error = "Product ID inválido" });
                }
            }
            else
            {
                return BadRequest(new { error = "Product ID é obrigatório" });
            }

            await _productService.DeleteProductAsync(productId, cancellationToken);
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
    [System.Text.Json.Serialization.JsonPropertyName("category")]
    public string? Category { get; set; }
}

public class UpdateProductRequestDto
{
    public Guid Id { get; set; }
    [System.Text.Json.Serialization.JsonPropertyName("product_id")]
    public string? product_id { get; set; }
    public string? Name { get; set; }
    public int? Price { get; set; }
    public string? Description { get; set; }
    public bool? Disabled { get; set; }
    [System.Text.Json.Serialization.JsonPropertyName("category")]
    public string? Category { get; set; }
}

public class SearchProductsResponseDto
{
    public List<ProductDto> products { get; set; } = new();
}
