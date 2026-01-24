using Backend.Application.DTOs.Product;
using Backend.Application.Interfaces;
using Backend.Domain.Entities;

namespace Backend.Application.Services;

public class ProductService
{
    private readonly IRepository<Product> _productRepository;

    public ProductService(IRepository<Product> productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<ProductDto> CreateProductAsync(string name, int price, string description, string category, CancellationToken cancellationToken = default)
    {
        var product = new Product
        {
            Name = name,
            Price = price,
            Description = description,
            Category = category ?? string.Empty
        };

        var createdProduct = await _productRepository.AddAsync(product, cancellationToken);
        
        return new ProductDto
        {
            Id = createdProduct.Id,
            Name = createdProduct.Name,
            Price = createdProduct.Price,
            Description = createdProduct.Description,
            Disabled = createdProduct.Disabled,
            Category = createdProduct.Category,
            CreatedAt = createdProduct.CreatedAt
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
            Category = p.Category,
            CreatedAt = p.CreatedAt
        });
    }

    public async Task<IEnumerable<ProductDto>> GetProductsByCategoryAsync(string category, CancellationToken cancellationToken = default)
    {
        var products = await _productRepository.FindAsync(p => p.Category == category, cancellationToken);

        return products.Select(p => new ProductDto
        {
            Id = p.Id,
            Name = p.Name,
            Price = p.Price,
            Description = p.Description,
            Disabled = p.Disabled,
            Category = p.Category,
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
            Category = product.Category,
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
            Category = p.Category,
            CreatedAt = p.CreatedAt
        });
    }

    public async Task<ProductDto> UpdateProductAsync(Guid id, string? name, int? price, string? description, bool? disabled, string? category, CancellationToken cancellationToken = default)
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
        if (category != null) product.Category = category;

        await _productRepository.UpdateAsync(product, cancellationToken);

        var updatedProduct = await _productRepository.GetByIdAsync(id, cancellationToken);

        return new ProductDto
        {
            Id = updatedProduct!.Id,
            Name = updatedProduct.Name,
            Price = updatedProduct.Price,
            Description = updatedProduct.Description,
            Disabled = updatedProduct.Disabled,
            Category = updatedProduct.Category,
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
