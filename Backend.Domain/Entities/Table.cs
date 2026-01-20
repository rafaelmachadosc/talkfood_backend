namespace Backend.Domain.Entities;

public class Table : BaseEntity
{
    public int Number { get; set; }
    public string QrCode { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
