using Backend.Application.DTOs.Product;
using Backend.Application.DTOs.Category;
using Backend.Application.Interfaces;
using Backend.Domain.Entities;

namespace Backend.Application.Services;

public class ProductService
{
    private readonly IRepository<Product> _productRepository;
    private readonly IRepository<Category> _categoryRepository;

    public ProductService(IRepository<Product> productRepository, IRepository<Category> categoryRepository)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
    }

    public async Task<ProductDto> CreateProductAsync(string name, int price, string description, Guid categoryId, CancellationToken cancellationToken = default)
    {
        var category = await _categoryRepository.GetByIdAsync(categoryId, cancellationToken);
        if (category == null)
        {
            throw new KeyNotFoundException("Categoria não encontrada");
        }

        var product = new Product
        {
            Name = name,
            Price = price,
            Description = description,
            CategoryId = categoryId
        };

        var createdProduct = await _productRepository.AddAsync(product, cancellationToken);

        // Recarregar produto com categoria
        var productWithCategory = await _productRepository.GetByIdAsync(createdProduct.Id, cancellationToken);
        
        return new ProductDto
        {
            Id = productWithCategory!.Id,
            Name = productWithCategory.Name,
            Price = productWithCategory.Price,
            Description = productWithCategory.Description,
            Disabled = productWithCategory.Disabled,
            CategoryId = productWithCategory.CategoryId,
            Category = productWithCategory.Category != null ? new CategoryDto
            {
                Id = productWithCategory.Category.Id,
                Name = productWithCategory.Category.Name
            } : null,
            CreatedAt = productWithCategory.CreatedAt
        };
    }

    public async Task<IEnumerable<ProductDto>> GetAllProductsAsync(bool? disabled = null, CancellationToken cancellationToken = default)
    {
        var products = disabled.HasValue
            ? await _productRepository.FindAsync(p => p.Disabled == disabled.Value, cancellationToken)
            : await _productRepository.GetAllAsync(cancellationToken);

        return products.Select(p => new ProductDto
        {
            Id = p.Id,
            Name = p.Name,
            Price = p.Price,
            Description = p.Description,
            Disabled = p.Disabled,
            CategoryId = p.CategoryId,
            Category = p.Category != null ? new CategoryDto
            {
                Id = p.Category.Id,
                Name = p.Category.Name
            } : null,
            CreatedAt = p.CreatedAt
        });
    }

    public async Task<IEnumerable<ProductDto>> GetProductsByCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        var products = await _productRepository.FindAsync(p => p.CategoryId == categoryId, cancellationToken);

        return products.Select(p => new ProductDto
        {
            Id = p.Id,
            Name = p.Name,
            Price = p.Price,
            Description = p.Description,
            Disabled = p.Disabled,
            CategoryId = p.CategoryId,
            Category = p.Category != null ? new CategoryDto
            {
                Id = p.Category.Id,
                Name = p.Category.Name
            } : null,
            CreatedAt = p.CreatedAt
        });
    }

    public async Task<ProductDto?> GetProductByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetByIdAsync(id, cancellationToken);
        if (product == null)
        {
            return null;
        }

        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Price = product.Price,
            Description = product.Description,
            Disabled = product.Disabled,
            CategoryId = product.CategoryId,
            Category = product.Category != null ? new CategoryDto
            {
                Id = product.Category.Id,
                Name = product.Category.Name
            } : null,
            CreatedAt = product.CreatedAt
        };
    }

    public async Task<IEnumerable<ProductDto>> SearchProductsAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        var allProducts = await _productRepository.GetAllAsync(cancellationToken);
        var searchLower = searchTerm.ToLower();

        var products = allProducts
            .Where(p => !p.Disabled && p.Name.ToLower().Contains(searchLower))
            .OrderBy(p => p.Name)
            .ToList();

        return products.Select(p => new ProductDto
        {
            Id = p.Id,
            Name = p.Name,
            Price = p.Price,
            Description = p.Description,
            Disabled = p.Disabled,
            CategoryId = p.CategoryId,
            Category = p.Category != null ? new CategoryDto
            {
                Id = p.Category.Id,
                Name = p.Category.Name
            } : null,
            CreatedAt = p.CreatedAt
        });
    }

    public async Task<ProductDto> UpdateProductAsync(Guid id, string? name, int? price, string? description, bool? disabled, Guid categoryId, CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetByIdAsync(id, cancellationToken);
        if (product == null)
        {
            throw new KeyNotFoundException("Produto não encontrado");
        }

        if (name != null) product.Name = name;
        if (price.HasValue) product.Price = price.Value;
        if (description != null) product.Description = description;
        if (disabled.HasValue) product.Disabled = disabled.Value;

        if (categoryId != Guid.Empty)
        {
            var category = await _categoryRepository.GetByIdAsync(categoryId, cancellationToken);
            if (category == null)
            {
                throw new KeyNotFoundException("Categoria não encontrada");
            }
            product.CategoryId = categoryId;
        }

        await _productRepository.UpdateAsync(product, cancellationToken);

        // Recarregar produto com categoria
        var updatedProduct = await _productRepository.GetByIdAsync(id, cancellationToken);

        return new ProductDto
        {
            Id = updatedProduct!.Id,
            Name = updatedProduct.Name,
            Price = updatedProduct.Price,
            Description = updatedProduct.Description,
            Disabled = updatedProduct.Disabled,
            CategoryId = updatedProduct.CategoryId,
            Category = updatedProduct.Category != null ? new CategoryDto
            {
                Id = updatedProduct.Category.Id,
                Name = updatedProduct.Category.Name
            } : null,
            CreatedAt = updatedProduct.CreatedAt
        };
    }

    public async Task DeleteProductAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetByIdAsync(id, cancellationToken);
        if (product == null)
        {
            throw new KeyNotFoundException("Produto não encontrado");
        }

        await _productRepository.DeleteAsync(product, cancellationToken);
    }
}
