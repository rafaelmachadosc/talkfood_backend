namespace Backend.Domain.Entities;

public class CashierMovement : BaseEntity
{
    public string Type { get; set; } = string.Empty; // "OPEN", "CLOSE", "SALE", "CHANGE_IN", "CHANGE_OUT"
    public int Amount { get; set; } // em centavos
    public string? Observation { get; set; }
    public string? PaymentMethod { get; set; } // "DINHEIRO", "PIX", "CARTAO_CREDITO", "CARTAO_DEBITO"
    public Guid CashierId { get; set; }
    public Cashier Cashier { get; set; } = null!;
}
