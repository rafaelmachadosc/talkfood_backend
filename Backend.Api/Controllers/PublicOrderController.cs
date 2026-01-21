using Backend.Application.DTOs.Order;
using Backend.Application.Services;
using Backend.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
            // Suporta order_id como Guid ou string
            Guid orderId;
            if (request.order_id != null)
            {
                if (request.order_id is Guid guid)
                {
                    orderId = guid;
                }
                else if (request.order_id is string str && Guid.TryParse(str, out var parsedGuid))
                {
                    orderId = parsedGuid;
                }
                else
                {
                    return BadRequest(new { error = "Order ID inválido" });
                }
            }
            else
            {
                return BadRequest(new { error = "Order ID é obrigatório" });
            }
            
            // Suporta product_id como Guid ou string
            Guid productId;
            if (request.product_id != null)
            {
                if (request.product_id is Guid guid)
                {
                    productId = guid;
                }
                else if (request.product_id is string str && Guid.TryParse(str, out var parsedGuid))
                {
                    productId = parsedGuid;
                }
                else
                {
                    return BadRequest(new { error = "Product ID inválido" });
                }
            }
            else
            {
                return BadRequest(new { error = "Product ID é obrigatório" });
            }
            
            // Suporta amount
            var amount = request.amount ?? throw new ArgumentException("Amount é obrigatório");

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
