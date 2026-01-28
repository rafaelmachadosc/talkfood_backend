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

public class CashierMovementDto
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public int Amount { get; set; }
    public string? Observation { get; set; }
    public Guid CashierId { get; set; }
    public DateTime CreatedAt { get; set; }
    public int Change { get; set; } = 0;
    public string? PaymentMethod { get; set; }
}

public class CashierSaleDto
{
    public string id { get; set; } = string.Empty;
    public string? order_id { get; set; }
    public int? table { get; set; }
    public string? commandNumber { get; set; }
    public string? name { get; set; }
    public int total { get; set; }
    public string payment_method { get; set; } = "DINHEIRO";
    public string createdAt { get; set; } = string.Empty;
}

public class CashierSaleItemDto
{
    public string id { get; set; } = string.Empty;
    public string product_name { get; set; } = string.Empty;
    public int amount { get; set; }
    public int unit_price { get; set; }
    public int total_price { get; set; }
}

public class CashierSaleDetailDto
{
    public string order_id { get; set; } = string.Empty;
    public int? table { get; set; }
    public string? commandNumber { get; set; }
    public string? name { get; set; }
    public int total { get; set; }
    public string? payment_method { get; set; }
    public int total_received { get; set; }
    public int remaining_amount { get; set; }
    public List<CashierSaleItemDto> items { get; set; } = new();
}
