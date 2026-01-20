using Backend.Domain.Enums;

namespace Backend.Domain.Entities;

public class Order : BaseEntity
{
    public int? Table { get; set; }
    public Guid? TableId { get; set; }
    public bool Status { get; set; } = false;
    public bool Draft { get; set; } = true;
    public string? Name { get; set; }
    public string? Phone { get; set; }
    public OrderType OrderType { get; set; } = OrderType.Mesa;
    public bool Viewed { get; set; } = false;
    public Table? TableRelation { get; set; }
    public ICollection<Item> Items { get; set; } = new List<Item>();
}
