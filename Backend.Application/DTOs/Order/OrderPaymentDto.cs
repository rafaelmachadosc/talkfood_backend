namespace Backend.Application.DTOs.Order;

public class OrderPaymentDto
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public int Amount { get; set; }
    public int ReceivedAmount { get; set; }
    public bool IsPartial { get; set; }
    public List<Guid> ItemIds { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

public class OrderPaymentsResponseDto
{
    public List<OrderPaymentDto> Payments { get; set; } = new();
    public int TotalReceived { get; set; }
    public int RemainingAmount { get; set; }
    public int OrderTotal { get; set; }
}

public class ReceivePartialPaymentRequestDto
{
    [System.Text.Json.Serialization.JsonPropertyName("order_id")]
    public string? order_id { get; set; }
    [System.Text.Json.Serialization.JsonPropertyName("item_ids")]
    public List<string>? item_ids { get; set; }
    [System.Text.Json.Serialization.JsonPropertyName("payment_method")]
    public string? payment_method { get; set; }
    [System.Text.Json.Serialization.JsonPropertyName("received_amount")]
    public int? received_amount { get; set; }
    [System.Text.Json.Serialization.JsonPropertyName("is_partial")]
    public bool? is_partial { get; set; }
}

public class ReceivePartialPaymentResponseDto
{
    public bool success { get; set; }
    public int remaining_amount { get; set; }
    public int total_received { get; set; }
}

public class AddItemsRequestDto
{
    [System.Text.Json.Serialization.JsonPropertyName("order_id")]
    public string? order_id { get; set; }
    public List<AddItemDto> items { get; set; } = new();
}

public class AddItemDto
{
    [System.Text.Json.Serialization.JsonPropertyName("product_id")]
    public string? product_id { get; set; }
    public int amount { get; set; }
}
