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
                CategoryId = p.CategoryId,
                Category = p.Category,
                CreatedAt = p.CreatedAt
            }).ToList()
        });
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

            // Validar se produto existe
            var existingProduct = await _productService.GetProductByIdAsync(productId, cancellationToken);
            if (existingProduct == null)
            {
                return NotFound(new { error = "Produto não encontrado" });
            }

            // Validar category_id se fornecido
            Guid? categoryId = null;
            if (!string.IsNullOrEmpty(request.category_id))
            {
                string categoryIdStr = request.category_id switch
                {
                    Guid g => g.ToString(),
                    string s => s,
                    System.Text.Json.JsonElement jsonElement => jsonElement.GetString() ?? string.Empty,
                    _ => request.category_id.ToString() ?? string.Empty
                };

                if (!string.IsNullOrWhiteSpace(categoryIdStr))
                {
                    if (!Guid.TryParse(categoryIdStr, out var parsedCategoryId))
                    {
                        return BadRequest(new { error = $"Category ID inválido: '{categoryIdStr}'. Deve ser um GUID válido." });
                    }
                    categoryId = parsedCategoryId;
                }
            }

            var product = await _productService.UpdateProductAsync(
                productId,
                request.Name,
                request.Price,
                request.Description,
                request.Disabled,
                categoryId,
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
    [System.Text.Json.Serialization.JsonPropertyName("product_id")]
    public string? product_id { get; set; }
    public string? Name { get; set; }
    public int? Price { get; set; }
    public string? Description { get; set; }
    public bool? Disabled { get; set; }
    [System.Text.Json.Serialization.JsonPropertyName("category_id")]
    public object? category_id { get; set; }
}

public class SearchProductsResponseDto
{
    public List<ProductDto> products { get; set; } = new();
}
