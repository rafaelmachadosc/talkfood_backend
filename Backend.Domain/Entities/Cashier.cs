namespace Backend.Domain.Entities;

public class Cashier : BaseEntity
{
    public bool IsOpen { get; set; } = false;
    public DateTime? OpenedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public Guid? OpenedBy { get; set; }
    public int InitialAmount { get; set; } = 0; // em centavos
    public int CurrentAmount { get; set; } = 0; // em centavos
    public int TotalSales { get; set; } = 0; // em centavos
    public int TotalOrders { get; set; } = 0;
    public ICollection<CashierMovement> Movements { get; set; } = new List<CashierMovement>();
}
