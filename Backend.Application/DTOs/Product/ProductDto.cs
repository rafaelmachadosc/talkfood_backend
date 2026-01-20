namespace Backend.Application.DTOs.Product;

public class ProductDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Price { get; set; }
    public string Description { get; set; } = string.Empty;
    public bool Disabled { get; set; }
    public Guid CategoryId { get; set; }
    public DateTime CreatedAt { get; set; }
}
