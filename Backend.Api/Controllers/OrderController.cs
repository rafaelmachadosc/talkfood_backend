using Backend.Application.DTOs.Order;
using Backend.Application.Services;
using Backend.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Route("api/orders")] // Rota alternativa para compatibilidade com frontend
[Authorize]
public class OrderController : ControllerBase
{
    // Endpoints públicos sem autenticação
}
{
    private readonly OrderService _orderService;

    public OrderController(OrderService orderService)
    {
        _orderService = orderService;
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
            // Se draft=false, retornar apenas não-draft (finalizados)
            var allOrders = await _orderService.GetAllOrdersAsync(cancellationToken);
            orders = allOrders.Where(o => !o.Draft);
        }
        else
        {
            orders = await _orderService.GetAllOrdersAsync(cancellationToken);
        }
        
        return Ok(orders);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<OrderDto>> GetOrder(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var order = await _orderService.GetOrderByIdAsync(id, cancellationToken);
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
    public async Task<ActionResult<OrderDto>> SendOrder(Guid id, CancellationToken cancellationToken)
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
    public async Task<ActionResult<OrderDto>> FinishOrder(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var order = await _orderService.FinishOrderAsync(id, cancellationToken);
            return Ok(order);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteOrder(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            await _orderService.DeleteOrderAsync(id, cancellationToken);
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
            // Suporta order_id no body (formato do frontend)
            Guid orderId;
            if (request.order_id != null)
            {
                // Pode ser Guid ou string
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
                    throw new ArgumentException("Order ID inválido");
                }
            }
            else
            {
                throw new ArgumentException("Order ID é obrigatório");
            }
            
            // Suporta product_id como Guid ou string
            Guid productId;
            if (request.product_id != null)
            {
                // Pode ser Guid ou string
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
                    throw new ArgumentException("Product ID inválido");
                }
            }
            else
            {
                throw new ArgumentException("Product ID é obrigatório");
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

    [HttpPost("public/{id}/add")]
    [HttpPost("public/add")] // Aceita também /api/order/public/add com order_id no body
    [AllowAnonymous]
    public async Task<ActionResult<OrderDto>> AddItemPublic(Guid? id, [FromBody] AddItemRequestDto request, CancellationToken cancellationToken)
    {
        try
        {
            // Suporta ID na URL ou no body
            Guid orderId;
            if (id.HasValue)
            {
                orderId = id.Value;
            }
            else if (request.order_id != null)
            {
                // Pode ser Guid ou string
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
                    throw new ArgumentException("Order ID inválido");
                }
            }
            else
            {
                throw new ArgumentException("Order ID é obrigatório");
            }
            
            // Suporta product_id como Guid ou string
            Guid productId;
            if (request.product_id != null)
            {
                // Pode ser Guid ou string
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
                    throw new ArgumentException("Product ID inválido");
                }
            }
            else
            {
                throw new ArgumentException("Product ID é obrigatório");
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

public class CreateOrderRequestDto
{
    public int? Table { get; set; }
    public string? Name { get; set; }
    public string? Phone { get; set; }
    [System.Text.Json.Serialization.JsonIgnore]
    public OrderType? OrderType { get; set; } // Ignorado no JSON, usado apenas internamente
    public string? orderType { get; set; } // camelCase do frontend - aceita "MESA" ou "BALCAO"
    [System.Text.Json.Serialization.JsonIgnore]
    public object? items { get; set; } // Ignorado - frontend envia array vazio, mas não é usado
}
