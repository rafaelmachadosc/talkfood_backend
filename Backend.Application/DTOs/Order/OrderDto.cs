using Backend.Domain.Enums;

namespace Backend.Application.DTOs.Order;

public class OrderDto
{
    public Guid Id { get; set; }
    public int? Table { get; set; }
    public Guid? TableId { get; set; }
    public bool Status { get; set; }
    public bool Draft { get; set; }
    public string? Name { get; set; }
    public string? Phone { get; set; }
    public string? CommandNumber { get; set; } // Número da comanda (string)
    [System.Text.Json.Serialization.JsonIgnore]
    public OrderType OrderType { get; set; } // Enum interno
    public string orderType { get; set; } = string.Empty; // String para frontend: "MESA" ou "BALCAO"
    public bool Viewed { get; set; }
    public List<ItemDto> Items { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

public class ItemDto
{
    public Guid Id { get; set; }
    public int Amount { get; set; }
    public Guid ProductId { get; set; }
    public ProductDto? Product { get; set; }
}

public class ProductDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Price { get; set; }
    public string Description { get; set; } = string.Empty;
}
