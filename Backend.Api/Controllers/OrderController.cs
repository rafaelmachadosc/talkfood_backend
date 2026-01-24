using Backend.Application.DTOs.Order;
using Backend.Application.Services;
using Backend.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Backend.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Route("api/orders")] // Rota alternativa para compatibilidade com frontend
[Authorize]
public class OrderController : ControllerBase
{
    private readonly OrderService _orderService;
    private readonly OrderPaymentService _paymentService;

    public OrderController(OrderService orderService, OrderPaymentService paymentService)
    {
        _orderService = orderService;
        _paymentService = paymentService;
    }

    [HttpPost]
    public async Task<ActionResult<OrderDto>> CreateOrder([FromBody] CreateOrderRequestDto request, CancellationToken cancellationToken)
    {
        try
        {
            // Converter string "MESA"/"BALCAO" para enum (aceita orderType em camelCase do frontend)
            OrderType orderType = request.OrderType ?? OrderType.Mesa;
            
            if (!string.IsNullOrEmpty(request.orderType))
            {
                if (request.orderType.Equals("MESA", StringComparison.OrdinalIgnoreCase))
                    orderType = OrderType.Mesa;
                else if (request.orderType.Equals("BALCAO", StringComparison.OrdinalIgnoreCase))
                    orderType = OrderType.Balcao;
            }

            // Validação: MESA precisa de table, BALCAO precisa de name
            if (orderType == OrderType.Mesa && !request.Table.HasValue)
            {
                return BadRequest(new { error = "Número da mesa é obrigatório para pedidos de mesa" });
            }
            
            if (orderType == OrderType.Balcao && string.IsNullOrWhiteSpace(request.Name))
            {
                return BadRequest(new { error = "Nome do cliente é obrigatório para pedidos de balcão" });
            }

            var order = await _orderService.CreateOrderAsync(
                request.Table,
                request.Name,
                request.Phone,
                request.CommandNumber,
                orderType,
                cancellationToken
            );
            return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message, details = ex.ToString() });
        }
    }

    [HttpPost("public")]
    [AllowAnonymous]
    public async Task<ActionResult<OrderDto>> CreatePublicOrder([FromBody] CreateOrderRequestDto request, CancellationToken cancellationToken)
    {
        try
        {
            // Converter string "MESA"/"BALCAO" para enum (aceita orderType em camelCase do frontend)
            OrderType orderType = request.OrderType ?? OrderType.Mesa;
            
            if (!string.IsNullOrEmpty(request.orderType))
            {
                if (request.orderType.Equals("MESA", StringComparison.OrdinalIgnoreCase))
                    orderType = OrderType.Mesa;
                else if (request.orderType.Equals("BALCAO", StringComparison.OrdinalIgnoreCase))
                    orderType = OrderType.Balcao;
            }

            var order = await _orderService.CreateOrderAsync(
                request.Table,
                request.Name,
                request.Phone,
                request.CommandNumber,
                orderType,
                cancellationToken
            );
            return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet]
    [HttpGet("orders")] // Suporta também /api/order/orders para compatibilidade
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetAllOrders([FromQuery] bool? draft, CancellationToken cancellationToken)
    {
        IEnumerable<OrderDto> orders;
        
        if (draft.HasValue && draft.Value)
        {
            orders = await _orderService.GetDraftOrdersAsync(cancellationToken);
        }
        else if (draft.HasValue && !draft.Value)
        {
            // Se draft=false, retornar apenas não-draft (enviados para produção)
            orders = await _orderService.GetNonDraftOrdersAsync(cancellationToken);
        }
        else
        {
            orders = await _orderService.GetAllOrdersAsync(cancellationToken);
        }
        
        return Ok(orders);
    }

    [HttpGet("{id}")]
    [HttpGet("detail")] // Suporta /api/order/detail?order_id={id}
    public async Task<ActionResult<OrderDto>> GetOrder(Guid? id, [FromQuery] string? order_id, CancellationToken cancellationToken)
    {
        try
        {
            Guid orderId;
            if (id.HasValue)
            {
                orderId = id.Value;
            }
            else if (!string.IsNullOrEmpty(order_id))
            {
                if (!Guid.TryParse(order_id, out orderId))
                {
                    return BadRequest(new { error = "Order ID inválido" });
                }
            }
            else
            {
                return BadRequest(new { error = "Order ID é obrigatório" });
            }

            var order = await _orderService.GetOrderByIdAsync(orderId, cancellationToken);
            return Ok(order);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    [HttpGet("public")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetPublicOrders([FromQuery] int? table, [FromQuery] string? phone, CancellationToken cancellationToken)
    {
        if (!table.HasValue)
        {
            return BadRequest(new { error = "Table é obrigatório" });
        }

        var orders = await _orderService.GetOrdersByTableAsync(table.Value, phone, cancellationToken);
        return Ok(orders);
    }

    [HttpPut("{id}/send")]
    [HttpPut("send")] // Suporta também /api/order/send com order_id no body
    public async Task<ActionResult<OrderDto>> SendOrder(Guid? id, [FromBody] SendOrderRequestDto? request, CancellationToken cancellationToken)
    {
        try
        {
            Guid orderId;
            if (id.HasValue)
            {
                orderId = id.Value;
            }
            else if (request != null && !string.IsNullOrEmpty(request.order_id))
            {
                if (!Guid.TryParse(request.order_id, out orderId))
                {
                    return BadRequest(new { error = "Order ID inválido" });
                }
            }
            else
            {
                return BadRequest(new { error = "Order ID é obrigatório" });
            }

            var order = await _orderService.SendOrderAsync(orderId, cancellationToken);
            return Ok(order);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    [HttpPut("public/{id}/send")]
    [AllowAnonymous]
    public async Task<ActionResult<OrderDto>> SendPublicOrder(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var order = await _orderService.SendOrderAsync(id, cancellationToken);
            return Ok(order);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    [HttpPut("{id}/finish")]
    [HttpPut("finish")] // Suporta também /api/order/finish com order_id no body
    public async Task<ActionResult<OrderDto>> FinishOrder(Guid? id, [FromBody] FinishOrderRequestDto? request, CancellationToken cancellationToken)
    {
        try
        {
            Guid orderId;
            if (id.HasValue)
            {
                orderId = id.Value;
            }
            else if (request != null && !string.IsNullOrEmpty(request.order_id))
            {
                if (!Guid.TryParse(request.order_id, out orderId))
                {
                    return BadRequest(new { error = "Order ID inválido" });
                }
            }
            else
            {
                return BadRequest(new { error = "Order ID é obrigatório" });
            }

            var order = await _orderService.FinishOrderAsync(orderId, cancellationToken);
            return Ok(order);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    [HttpPut("viewed")] // PUT /api/order/viewed com order_id no body
    public async Task<ActionResult<OrderDto>> MarkAsViewed([FromBody] ViewedOrderRequestDto request, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrEmpty(request.order_id))
            {
                return BadRequest(new { error = "Order ID é obrigatório" });
            }

            if (!Guid.TryParse(request.order_id, out var orderId))
            {
                return BadRequest(new { error = "Order ID inválido" });
            }

            var order = await _orderService.MarkAsViewedAsync(orderId, cancellationToken);
            return Ok(order);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    [HttpPut("{id}/command-number")]
    [HttpPut("command-number")]
    public async Task<ActionResult<OrderDto>> UpdateCommandNumber(Guid? id, [FromBody] UpdateCommandNumberRequestDto? request, CancellationToken cancellationToken)
    {
        try
        {
            Guid orderId;
            if (id.HasValue)
            {
                orderId = id.Value;
            }
            else if (request != null && !string.IsNullOrEmpty(request.order_id))
            {
                if (!Guid.TryParse(request.order_id, out orderId))
                {
                    return BadRequest(new { error = "Order ID inválido" });
                }
            }
            else
            {
                return BadRequest(new { error = "Order ID é obrigatório" });
            }

            var order = await _orderService.UpdateCommandNumberAsync(orderId, request?.commandNumber, cancellationToken);
            return Ok(order);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    [HttpPut] // Suporta também /api/order com order_id no body
    public async Task<ActionResult<OrderDto>> UpdateOrder(Guid? id, [FromBody] UpdateOrderRequestDto? request, CancellationToken cancellationToken)
    {
        try
        {
            Guid orderId;
            if (id.HasValue)
            {
                orderId = id.Value;
            }
            else if (request != null && !string.IsNullOrEmpty(request.order_id))
            {
                if (!Guid.TryParse(request.order_id, out orderId))
                {
                    return BadRequest(new { error = "Order ID inválido" });
                }
            }
            else
            {
                return BadRequest(new { error = "Order ID é obrigatório" });
            }

            int? commandNumber = null;
            if (request?.commandNumber != null)
            {
                if (request.commandNumber is int i)
                {
                    commandNumber = i;
                }
                else if (request.commandNumber is string strValue && !string.IsNullOrWhiteSpace(strValue))
                {
                    if (int.TryParse(strValue, out var parsed))
                    {
                        commandNumber = parsed;
                    }
                }
                else if (request.commandNumber is System.Text.Json.JsonElement jsonElement)
                {
                    if (jsonElement.ValueKind == System.Text.Json.JsonValueKind.Number)
                    {
                        commandNumber = jsonElement.GetInt32();
                    }
                    else if (jsonElement.ValueKind == System.Text.Json.JsonValueKind.String)
                    {
                        var strValue2 = jsonElement.GetString();
                        if (!string.IsNullOrWhiteSpace(strValue2) && int.TryParse(strValue2, out var parsed))
                        {
                            commandNumber = parsed;
                        }
                    }
                }
            }

            var order = await _orderService.UpdateOrderInfoAsync(orderId, request?.name, commandNumber, cancellationToken);
            return Ok(order);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    [HttpGet("by-command-or-name")]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetOrdersByCommandOrName([FromQuery] int? commandNumber, [FromQuery] string? name, CancellationToken cancellationToken)
    {
        var orders = await _orderService.GetOrdersByCommandOrNameAsync(commandNumber, name, cancellationToken);
        return Ok(orders);
    }

    [HttpDelete("{id}")]
    [HttpDelete] // Suporta também /api/order?order_id={id}
    public async Task<ActionResult> DeleteOrder(Guid? id, [FromQuery] string? order_id, CancellationToken cancellationToken)
    {
        try
        {
            Guid orderId;
            if (id.HasValue)
            {
                orderId = id.Value;
            }
            else if (!string.IsNullOrEmpty(order_id))
            {
                if (!Guid.TryParse(order_id, out orderId))
                {
                    return BadRequest(new { error = "Order ID inválido" });
                }
            }
            else
            {
                return BadRequest(new { error = "Order ID é obrigatório" });
            }

            await _orderService.DeleteOrderAsync(orderId, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    [HttpPost("{id}/add")]
    public async Task<ActionResult<OrderDto>> AddItem(Guid id, [FromBody] AddItemRequestDto request, CancellationToken cancellationToken)
    {
        try
        {
            // Suporta ProductId em PascalCase ou camelCase
            var productId = request.ProductId != Guid.Empty ? request.ProductId : (request.productId ?? throw new ArgumentException("Product ID é obrigatório"));
            
            // Suporta Amount em PascalCase ou camelCase
            var amount = request.Amount != 0 ? request.Amount : (request.amount ?? throw new ArgumentException("Amount é obrigatório"));

            var order = await _orderService.AddItemAsync(id, productId, amount, cancellationToken);
            return Ok(order);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("add")]
    public async Task<ActionResult<OrderDto>> AddItemFromBody([FromBody] AddItemRequestDto request, CancellationToken cancellationToken)
    {
        try
        {
            var orderId = ParseGuidFromObject(request.order_id, "Order ID");
            var productId = ParseGuidFromObject(request.product_id, "Product ID");
            var amount = ParseIntFromObject(request.amount, "Amount");

            var order = await _orderService.AddItemAsync(orderId, productId, amount, cancellationToken);
            return Ok(order);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("public/{id}/add")]
    [HttpPost("public/add")] // Aceita também /api/order/public/add com order_id no body
    [AllowAnonymous]
    public async Task<ActionResult<OrderDto>> AddItemPublic(Guid? id, [FromBody] AddItemRequestDto request, CancellationToken cancellationToken)
    {
        try
        {
            var orderId = id ?? ParseGuidFromObject(request.order_id, "Order ID");
            var productId = ParseGuidFromObject(request.product_id, "Product ID");
            var amount = ParseIntFromObject(request.amount, "Amount");

            var order = await _orderService.AddItemAsync(orderId, productId, amount, cancellationToken);
            return Ok(order);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    private Guid ParseGuidFromObject(object? obj, string fieldName)
    {
        if (obj == null) throw new ArgumentException($"{fieldName} é obrigatório");

        if (obj is Guid guid) return guid;
        if (obj is string str && Guid.TryParse(str, out var parsedGuid)) return parsedGuid;
        if (obj is JsonElement jsonElement)
        {
            if (jsonElement.ValueKind == JsonValueKind.String && Guid.TryParse(jsonElement.GetString(), out parsedGuid)) return parsedGuid;
        }
        throw new ArgumentException($"{fieldName} inválido");
    }

    private int ParseIntFromObject(object? obj, string fieldName)
    {
        if (obj == null) throw new ArgumentException($"{fieldName} é obrigatório");

        if (obj is int i) return i;
        if (obj is long l) return (int)l; // JSON numbers can be long
        if (obj is string str && int.TryParse(str, out var parsedInt)) return parsedInt;
        if (obj is JsonElement jsonElement)
        {
            if (jsonElement.ValueKind == JsonValueKind.Number) return jsonElement.GetInt32();
            if (jsonElement.ValueKind == JsonValueKind.String && int.TryParse(jsonElement.GetString(), out parsedInt)) return parsedInt;
        }
        throw new ArgumentException($"{fieldName} inválido");
    }

    [HttpDelete("remove/{itemId}")]
    public async Task<ActionResult> RemoveItem(Guid itemId, CancellationToken cancellationToken)
    {
        try
        {
            await _orderService.RemoveItemAsync(itemId, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    [HttpPost("receive-partial")]
    public async Task<ActionResult<ReceivePartialPaymentResponseDto>> ReceivePartialPayment([FromBody] ReceivePartialPaymentRequestDto request, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrEmpty(request.order_id))
            {
                return BadRequest(new { error = "Order ID é obrigatório" });
            }

            if (!Guid.TryParse(request.order_id, out var orderId))
            {
                return BadRequest(new { error = "Order ID inválido" });
            }

            if (string.IsNullOrEmpty(request.payment_method))
            {
                return BadRequest(new { error = "Payment method é obrigatório" });
            }

            if (!request.received_amount.HasValue || request.received_amount.Value <= 0)
            {
                return BadRequest(new { error = "Received amount deve ser maior que zero" });
            }

            var itemIds = new List<Guid>();
            if (request.item_ids != null)
            {
                foreach (var itemIdStr in request.item_ids)
                {
                    if (Guid.TryParse(itemIdStr, out var itemId))
                    {
                        itemIds.Add(itemId);
                    }
                }
            }

            var result = await _paymentService.ReceivePartialPaymentAsync(
                orderId,
                itemIds.Any() ? itemIds : null,
                request.payment_method,
                request.received_amount.Value,
                cancellationToken
            );

            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("{orderId}/payments")]
    public async Task<ActionResult<OrderPaymentsResponseDto>> GetOrderPayments(Guid orderId, CancellationToken cancellationToken)
    {
        try
        {
            var payments = await _paymentService.GetOrderPaymentsAsync(orderId, cancellationToken);
            return Ok(payments);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    [HttpPost("add-items")]
    public async Task<ActionResult<OrderDto>> AddMultipleItems([FromBody] AddItemsRequestDto request, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrEmpty(request.order_id))
            {
                return BadRequest(new { error = "Order ID é obrigatório" });
            }

            if (!Guid.TryParse(request.order_id, out var orderId))
            {
                return BadRequest(new { error = "Order ID inválido" });
            }

            if (request.items == null || !request.items.Any())
            {
                return BadRequest(new { error = "Pelo menos um item é obrigatório" });
            }

            var items = new List<(Guid productId, int amount)>();
            foreach (var item in request.items)
            {
                if (string.IsNullOrEmpty(item.product_id))
                {
                    return BadRequest(new { error = "Product ID é obrigatório para todos os itens" });
                }

                if (!Guid.TryParse(item.product_id, out var productId))
                {
                    return BadRequest(new { error = $"Product ID inválido: {item.product_id}" });
                }

                if (item.amount <= 0)
                {
                    return BadRequest(new { error = "Amount deve ser maior que zero" });
                }

                items.Add((productId, item.amount));
            }

            var order = await _orderService.AddMultipleItemsAsync(orderId, items, cancellationToken);
            return Ok(order);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}

public class AddItemRequestDto
{
    [System.Text.Json.Serialization.JsonIgnore]
    public Guid ProductId { get; set; } // Ignorado no JSON
    [System.Text.Json.Serialization.JsonIgnore]
    public Guid? productId { get; set; } // Ignorado no JSON
    [System.Text.Json.Serialization.JsonPropertyName("product_id")]
    public object? product_id { get; set; } // snake_case do frontend - pode ser Guid ou string
    [System.Text.Json.Serialization.JsonIgnore]
    public int Amount { get; set; } // Ignorado no JSON
    [System.Text.Json.Serialization.JsonPropertyName("amount")]
    public int? amount { get; set; } // camelCase do frontend
    [System.Text.Json.Serialization.JsonPropertyName("order_id")]
    public object? order_id { get; set; } // snake_case do frontend - pode ser Guid ou string
}

public class SendOrderRequestDto
{
    [System.Text.Json.Serialization.JsonPropertyName("order_id")]
    public string? order_id { get; set; }
    public string? name { get; set; } // opcional
}

public class FinishOrderRequestDto
{
    [System.Text.Json.Serialization.JsonPropertyName("order_id")]
    public string? order_id { get; set; }
}

public class ViewedOrderRequestDto
{
    [System.Text.Json.Serialization.JsonPropertyName("order_id")]
    public string? order_id { get; set; }
}

public class UpdateCommandNumberRequestDto
{
    [System.Text.Json.Serialization.JsonPropertyName("order_id")]
    public string? order_id { get; set; }
    [System.Text.Json.Serialization.JsonPropertyName("commandNumber")]
    public int? commandNumber { get; set; }
}

public class UpdateOrderRequestDto
{
    [System.Text.Json.Serialization.JsonPropertyName("order_id")]
    public string? order_id { get; set; }
    [System.Text.Json.Serialization.JsonPropertyName("name")]
    public string? name { get; set; }
    [System.Text.Json.Serialization.JsonPropertyName("commandNumber")]
    public object? commandNumber { get; set; } // Pode ser int, string ou null
}

public class CreateOrderRequestDto
{
    public int? Table { get; set; }
    public string? Name { get; set; }
    public string? Phone { get; set; }
    [System.Text.Json.Serialization.JsonPropertyName("commandNumber")]
    public int? CommandNumber { get; set; } // Número da comanda
    [System.Text.Json.Serialization.JsonIgnore]
    public OrderType? OrderType { get; set; } // Ignorado no JSON, usado apenas internamente
    public string? orderType { get; set; } // camelCase do frontend - aceita "MESA" ou "BALCAO"
    [System.Text.Json.Serialization.JsonIgnore]
    public object? items { get; set; } // Ignorado - frontend envia array vazio, mas não é usado
}
