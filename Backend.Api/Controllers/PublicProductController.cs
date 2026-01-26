using Backend.Application.DTOs.Product;
using Backend.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Controllers;

[ApiController]
[Route("api/public/products")]
[AllowAnonymous]
public class PublicProductController : ControllerBase
{
    private readonly ProductService _productService;

    public PublicProductController(ProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetAllProducts([FromQuery] bool? disabled, CancellationToken cancellationToken)
    {
        var products = await _productService.GetAllProductsAsync(disabled ?? false, cancellationToken);
        return Ok(products);
    }
}
