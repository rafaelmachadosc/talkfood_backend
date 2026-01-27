using Backend.Application.DTOs.Print;
using Backend.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Controllers;

[ApiController]
[Route("api/public/print")]
[AllowAnonymous]
public class PublicPrintController : ControllerBase
{
    private readonly PrintService _printService;
    private readonly ILogger<PublicPrintController> _logger;

    public PublicPrintController(PrintService printService, ILogger<PublicPrintController> logger)
    {
        _printService = printService;
        _logger = logger;
    }

    [HttpPost("receipt")]
    [Produces("application/json")]
    public async Task<ActionResult<PrintResponseDto>> PrintReceipt([FromBody] PrintRequestDto request, CancellationToken cancellationToken)
    {
        try
        {
            var receipt = await _printService.GenerateReceiptAsync(
                request.OrderId,
                request.ReceiptType,
                cancellationToken
            );

            var rawData = await _printService.FormatReceiptForPrintAsync(receipt, null, cancellationToken);

            return Ok(new PrintResponseDto
            {
                Success = true,
                Message = "Cupom gerado com sucesso",
                Receipt = receipt,
                RawData = rawData
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao gerar cupom: {Message}", ex.Message);
            return StatusCode(500, new { error = "Erro ao gerar cupom", details = ex.Message });
        }
    }

    [HttpGet("receipt/{orderId}")]
    [Produces("application/json")]
    public async Task<ActionResult<PrintResponseDto>> GetReceipt(Guid orderId, [FromQuery] string receiptType = "ORDER", CancellationToken cancellationToken = default)
    {
        try
        {
            var receipt = await _printService.GenerateReceiptAsync(orderId, receiptType, cancellationToken);
            var rawData = await _printService.FormatReceiptForPrintAsync(receipt, null, cancellationToken);

            return Ok(new PrintResponseDto
            {
                Success = true,
                Message = "Cupom gerado com sucesso",
                Receipt = receipt,
                RawData = rawData
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao gerar cupom: {Message}", ex.Message);
            return StatusCode(500, new { error = "Erro ao gerar cupom", details = ex.Message });
        }
    }
}
