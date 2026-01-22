namespace Backend.Domain.Entities;

public class OrderPayment : BaseEntity
{
    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;
    public string PaymentMethod { get; set; } = string.Empty; // "DINHEIRO", "PIX", "CARTAO_CREDITO", "CARTAO_DEBITO"
    public int Amount { get; set; } // em centavos
    public int ReceivedAmount { get; set; } // em centavos
    public bool IsPartial { get; set; } = false;
    public string? ItemIdsJson { get; set; } // JSON array de GUIDs dos itens pagos (se parcial)
}
