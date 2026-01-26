using Backend.Application.DTOs.Order;
using Backend.Application.Services;
using Backend.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Backend.Api.Controllers;

[ApiController]
[Route("api/public/order")]
[AllowAnonymous]
public class PublicOrderController : ControllerBase
{
    private readonly OrderService _orderService;

    public PublicOrderController(OrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpPost]
    public async Task<ActionResult<OrderDto>> CreateOrder([FromBody] CreateOrderRequestDto request, CancellationToken cancellationToken)
    {
        try
        {
            // Converter string "MESA"/"BALCAO" para enum
            OrderType orderType = OrderType.Mesa;
            
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
            return CreatedAtAction(nameof(GetOrderDetail), new { order_id = order.Id }, order);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("detail")]
    public async Task<ActionResult<OrderDto>> GetOrderDetail([FromQuery] string order_id, CancellationToken cancellationToken)
    {
        try
        {
            if (!Guid.TryParse(order_id, out var orderId))
            {
                return BadRequest(new { error = "Order ID inválido" });
            }

            var order = await _orderService.GetOrderByIdAsync(orderId, cancellationToken);
            return Ok(order);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    [HttpGet("orders")]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetOrders([FromQuery] int? table, [FromQuery] bool? draft, CancellationToken cancellationToken)
    {
        if (!table.HasValue)
        {
            return BadRequest(new { error = "Table é obrigatório" });
        }

        var orders = await _orderService.GetOrdersByTableAsync(table.Value, null, cancellationToken);
        
        if (draft.HasValue)
        {
            orders = orders.Where(o => o.Draft == draft.Value);
        }
        
        return Ok(orders);
    }

    [HttpPost("add")]
    public async Task<ActionResult<OrderDto>> AddItem([FromBody] AddItemRequestDto request, CancellationToken cancellationToken)
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

    [HttpPut("send")]
    public async Task<ActionResult<OrderDto>> SendOrder([FromQuery] string order_id, CancellationToken cancellationToken)
    {
        try
        {
            if (!Guid.TryParse(order_id, out var orderId))
            {
                return BadRequest(new { error = "Order ID inválido" });
            }

            var order = await _orderService.SendOrderAsync(orderId, cancellationToken);
            return Ok(order);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    [HttpDelete("remove")]
    public async Task<ActionResult> RemoveItem([FromQuery] string item_id, CancellationToken cancellationToken)
    {
        try
        {
            if (!Guid.TryParse(item_id, out var itemId))
            {
                return BadRequest(new { error = "Item ID inválido" });
            }

            await _orderService.RemoveItemAsync(itemId, cancellationToken);
            return NoContent();
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

    [HttpPut("{id}")]
    [HttpPut] // Suporta também /api/public/order com order_id no body
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

            string? commandNumber = null;
            if (request?.commandNumber != null)
            {
                if (request.commandNumber is string strValue)
                {
                    commandNumber = strValue.Trim();
                }
                else if (request.commandNumber is System.Text.Json.JsonElement jsonElement)
                {
                    if (jsonElement.ValueKind == System.Text.Json.JsonValueKind.String)
                    {
                        commandNumber = jsonElement.GetString()?.Trim();
                    }
                    else if (jsonElement.ValueKind == System.Text.Json.JsonValueKind.Number)
                    {
                        commandNumber = jsonElement.GetInt32().ToString();
                    }
                }
                else
                {
                    commandNumber = request.commandNumber.ToString()?.Trim();
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
}

