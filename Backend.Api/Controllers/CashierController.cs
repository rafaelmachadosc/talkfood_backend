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

            var amount = request.InitialAmount != 0 ? request.InitialAmount : (request.initialAmount ?? 0);
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
}

public class OpenCashierRequestDto
{
    public int InitialAmount { get; set; }
    public int? initialAmount { get; set; } // camelCase do frontend
}
