using Backend.Application.DTOs.Category;
using Backend.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Route("api/category")]
public class CategoryController : ControllerBase
{
    private readonly CategoryService _categoryService;
    private readonly ILogger<CategoryController> _logger;

    public CategoryController(CategoryService categoryService, ILogger<CategoryController> logger)
    {
        _categoryService = categoryService;
        _logger = logger;
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<CategoryDto>> CreateCategory([FromBody] CreateCategoryRequestDto request, CancellationToken cancellationToken)
    {
        try
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Name))
            {
                return BadRequest(new { error = "Nome da categoria é obrigatório" });
            }

            var category = await _categoryService.CreateCategoryAsync(request.Name.Trim(), cancellationToken);
            return Ok(category);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message, details = ex.ToString() });
        }
    }

    [HttpGet]
    [Authorize]
    public async Task<ActionResult<IEnumerable<CategoryDto>>> GetAllCategories(CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("GET /api/category - Iniciando busca de categorias");
            var categories = await _categoryService.GetAllCategoriesAsync(cancellationToken);
            _logger.LogInformation($"GET /api/category - Encontradas {categories?.Count() ?? 0} categorias");
            return Ok(categories ?? new List<CategoryDto>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GET /api/category - Erro ao buscar categorias");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("public")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<CategoryDto>>> GetAllCategoriesPublic(CancellationToken cancellationToken)
    {
        try
        {
            var categories = await _categoryService.GetAllCategoriesAsync(cancellationToken);
            return Ok(categories);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message, details = ex.ToString() });
        }
    }
}

public class CreateCategoryRequestDto
{
    [System.Text.Json.Serialization.JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}
