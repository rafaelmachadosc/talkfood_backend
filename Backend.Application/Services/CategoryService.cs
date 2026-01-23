using Backend.Application.DTOs.Category;
using Backend.Application.Interfaces;
using Backend.Domain.Entities;

namespace Backend.Application.Services;

public class CategoryService
{
    private readonly IRepository<Category> _categoryRepository;

    public CategoryService(IRepository<Category> categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<CategoryDto> CreateCategoryAsync(string name, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Nome da categoria não pode ser vazio", nameof(name));
        }

        var category = new Category
        {
            Name = name.Trim()
        };

        var createdCategory = await _categoryRepository.AddAsync(category, cancellationToken);

        return new CategoryDto
        {
            Id = createdCategory.Id,
            Name = createdCategory.Name,
            CreatedAt = createdCategory.CreatedAt
        };
    }

    public async Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var categories = await _categoryRepository.GetAllAsync(cancellationToken);

            return categories.Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                CreatedAt = c.CreatedAt
            });
        }
        catch (Exception)
        {
            return new List<CategoryDto>();
        }
    }
}
