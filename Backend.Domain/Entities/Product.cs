namespace Backend.Domain.Entities;

public class Product : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public int Price { get; set; } // em centavos
    public string Description { get; set; } = string.Empty;
    public bool Disabled { get; set; } = false;
    public string Category { get; set; } = string.Empty;
    public ICollection<Item> Items { get; set; } = new List<Item>();
}
