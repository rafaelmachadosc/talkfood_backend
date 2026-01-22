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
}
