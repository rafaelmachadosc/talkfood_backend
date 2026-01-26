namespace Backend.Domain.Entities;

public class DailySales : BaseEntity
{
    public DateTime Date { get; set; } // Data do dia (apenas data, sem hora)
    public int TotalSales { get; set; } = 0; // Total de vendas do dia em centavos
    public int TotalOrders { get; set; } = 0; // Total de pedidos do dia
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}
