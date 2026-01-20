using Backend.Application.DTOs.Table;
using Backend.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TableController : ControllerBase
{
    private readonly TableService _tableService;

    public TableController(TableService tableService)
    {
        _tableService = tableService;
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<TableDto>> CreateTable([FromBody] CreateTableRequestDto request, CancellationToken cancellationToken)
    {
        try
        {
            var table = await _tableService.CreateTableAsync(request.Number, cancellationToken);
            return CreatedAtAction(nameof(GetTable), new { id = table.Id }, table);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet]
    [Authorize]
    public async Task<ActionResult<IEnumerable<TableDto>>> GetAllTables(CancellationToken cancellationToken)
    {
        var tables = await _tableService.GetAllTablesAsync(cancellationToken);
        return Ok(tables);
    }

    [HttpGet("qr/{qrCode}")]
    [AllowAnonymous]
    public async Task<ActionResult<TableDto>> GetTableByQrCode(string qrCode, CancellationToken cancellationToken)
    {
        try
        {
            var table = await _tableService.GetTableByQrCodeAsync(qrCode, cancellationToken);
            return Ok(table);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<ActionResult<TableDto>> GetTable(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var tables = await _tableService.GetAllTablesAsync(cancellationToken);
            var table = tables.FirstOrDefault(t => t.Id == id);
            if (table == null)
            {
                return NotFound(new { error = "Mesa não encontrada" });
            }
            return Ok(table);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<TableDto>> UpdateTable(Guid id, [FromBody] UpdateTableRequestDto request, CancellationToken cancellationToken)
    {
        try
        {
            var table = await _tableService.UpdateTableAsync(id, request.Number, request.IsActive, cancellationToken);
            return Ok(table);
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

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> DeleteTable(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            await _tableService.DeleteTableAsync(id, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }
}

public class CreateTableRequestDto
{
    public int Number { get; set; }
}

public class UpdateTableRequestDto
{
    public int? Number { get; set; }
    public bool? IsActive { get; set; }
}
