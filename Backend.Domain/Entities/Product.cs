namespace Backend.Domain.Entities;

public class Product : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public int Price { get; set; } // em centavos
    public string Description { get; set; } = string.Empty;
    public bool Disabled { get; set; } = false;
    public Guid CategoryId { get; set; }
    public Category Category { get; set; } = null!;
    public ICollection<Item> Items { get; set; } = new List<Item>();
}
