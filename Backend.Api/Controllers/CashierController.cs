using Backend.Application.DTOs.Cashier;
using Backend.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Backend.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Route("api/caixa")] // Rota alternativa em português
[Authorize]
public class CashierController : ControllerBase
{
    private readonly CashierService _cashierService;

    public CashierController(CashierService cashierService)
    {
        _cashierService = cashierService;
    }

    [HttpGet("status")]
    public async Task<ActionResult<CashierDto>> GetCashierStatus(CancellationToken cancellationToken)
    {
        var status = await _cashierService.GetCashierStatusAsync(cancellationToken);
        return Ok(status);
    }

    [HttpPost("open")]
    public async Task<ActionResult<CashierDto>> OpenCashier([FromBody] OpenCashierRequestDto request, CancellationToken cancellationToken)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return Unauthorized(new { error = "Token inválido" });
            }

            // Aceita initialAmount (camelCase) do frontend
            var amount = request.initialAmount ?? request.InitialAmount;
            if (amount <= 0)
            {
                return BadRequest(new { error = "Valor inicial deve ser maior que zero" });
            }
            var cashier = await _cashierService.OpenCashierAsync(amount, userId, cancellationToken);
            return Ok(cashier);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("close")]
    public async Task<ActionResult<CashierDto>> CloseCashier(CancellationToken cancellationToken)
    {
        try
        {
            var cashier = await _cashierService.CloseCashierAsync(cancellationToken);
            return Ok(cashier);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("receive")]
    public async Task<ActionResult<ReceivePaymentResponseDto>> ReceivePayment([FromBody] ReceivePaymentRequestDto request, CancellationToken cancellationToken)
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

            if (request.amount <= 0)
            {
                return BadRequest(new { error = "Valor deve ser maior que zero" });
            }

            if (request.received_amount.HasValue && request.received_amount.Value < request.amount)
            {
                return BadRequest(new { error = "Valor recebido não pode ser menor que o valor total" });
            }

            var movement = await _cashierService.ReceivePaymentAsync(
                orderId,
                request.amount,
                request.payment_method ?? "DINHEIRO",
                request.received_amount,
                cancellationToken
            );

            return Ok(new ReceivePaymentResponseDto
            {
                id = movement.Id.ToString(),
                order_id = request.order_id,
                amount = movement.Amount,
                payment_method = request.payment_method ?? "DINHEIRO",
                received_amount = request.received_amount ?? request.amount,
                change = movement.Change,
                createdAt = movement.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}

public class OpenCashierRequestDto
{
    [System.Text.Json.Serialization.JsonIgnore]
    public int InitialAmount { get; set; }
    [System.Text.Json.Serialization.JsonPropertyName("initialAmount")]
    public int? initialAmount { get; set; } // camelCase do frontend
}

public class ReceivePaymentRequestDto
{
    [System.Text.Json.Serialization.JsonPropertyName("order_id")]
    public string? order_id { get; set; }
    public int amount { get; set; }
    [System.Text.Json.Serialization.JsonPropertyName("payment_method")]
    public string? payment_method { get; set; }
    [System.Text.Json.Serialization.JsonPropertyName("received_amount")]
    public int? received_amount { get; set; }
}

public class ReceivePaymentResponseDto
{
    public string id { get; set; } = string.Empty;
    [System.Text.Json.Serialization.JsonPropertyName("order_id")]
    public string order_id { get; set; } = string.Empty;
    public int amount { get; set; }
    [System.Text.Json.Serialization.JsonPropertyName("payment_method")]
    public string payment_method { get; set; } = string.Empty;
    [System.Text.Json.Serialization.JsonPropertyName("received_amount")]
    public int received_amount { get; set; }
    public int change { get; set; }
    [System.Text.Json.Serialization.JsonPropertyName("createdAt")]
    public string createdAt { get; set; } = string.Empty;
}
