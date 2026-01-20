namespace Backend.Application.DTOs.Cashier;

public class CashierDto
{
    public Guid Id { get; set; }
    public bool IsOpen { get; set; }
    public DateTime? OpenedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public Guid? OpenedBy { get; set; }
    public int InitialAmount { get; set; }
    public int CurrentAmount { get; set; }
    public int TotalSales { get; set; }
    public int TotalOrders { get; set; }
}
