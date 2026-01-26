using Backend.Application.DTOs.Print;
using Backend.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PrintController : ControllerBase
{
    private readonly PrintService _printService;
    private readonly ILogger<PrintController> _logger;

    public PrintController(PrintService printService, ILogger<PrintController> logger)
    {
        _printService = printService;
        _logger = logger;
    }

    [HttpPost("receipt")]
    public async Task<ActionResult<PrintResponseDto>> PrintReceipt([FromBody] PrintRequestDto request, CancellationToken cancellationToken)
    {
        try
        {
            var receipt = await _printService.GenerateReceiptAsync(
                request.OrderId,
                request.ReceiptType,
                cancellationToken
            );

            // Buscar impressora (se especificada) ou usar padrão
            var printers = await _printService.GetAllPrintersAsync(cancellationToken);
            var printer = request.PrinterId.HasValue
                ? printers.FirstOrDefault(p => p.Id == request.PrinterId.Value && p.IsActive)
                : printers.FirstOrDefault(p => p.IsActive && p.AutoPrint);

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
    public async Task<ActionResult<PrintResponseDto>> GetReceipt(Guid orderId, [FromQuery] string receiptType = "ORDER", CancellationToken cancellationToken = default)
    {
        try
        {
            var receipt = await _printService.GenerateReceiptAsync(orderId, receiptType, cancellationToken);
            
            var printers = await _printService.GetAllPrintersAsync(cancellationToken);
            var printer = printers.FirstOrDefault(p => p.IsActive);

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

    [HttpGet("printers")]
    public async Task<ActionResult<IEnumerable<PrinterDto>>> GetAllPrinters(CancellationToken cancellationToken)
    {
        try
        {
            var printers = await _printService.GetAllPrintersAsync(cancellationToken);
            return Ok(printers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar impressoras: {Message}", ex.Message);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPost("printers")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<PrinterDto>> CreatePrinter([FromBody] CreatePrinterRequestDto request, CancellationToken cancellationToken)
    {
        try
        {
            var printer = await _printService.CreatePrinterAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetAllPrinters), new { id = printer.Id }, printer);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar impressora: {Message}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("printers")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<PrinterDto>> UpdatePrinter([FromBody] UpdatePrinterRequestDto request, CancellationToken cancellationToken)
    {
        try
        {
            var printer = await _printService.UpdatePrinterAsync(request, cancellationToken);
            return Ok(printer);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar impressora: {Message}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("printers/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> DeletePrinter(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            await _printService.DeletePrinterAsync(id, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao deletar impressora: {Message}", ex.Message);
            return StatusCode(500, new { error = ex.Message });
        }
    }
}
