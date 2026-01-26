using Backend.Application.DTOs.Analytics;
using Backend.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AnalyticsController : ControllerBase
{
    private readonly AnalyticsService _analyticsService;

    public AnalyticsController(AnalyticsService analyticsService)
    {
        _analyticsService = analyticsService;
    }

    [HttpGet("metrics")]
    public async Task<ActionResult<MetricsDto>> GetMetrics(CancellationToken cancellationToken)
    {
        try
        {
            var metrics = await _analyticsService.GetMetricsAsync(cancellationToken);
            return Ok(metrics);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("daily-sales")]
    public async Task<ActionResult<DailySalesResponseDto>> GetDailySales([FromQuery] int? days, CancellationToken cancellationToken)
    {
        try
        {
            var dailySales = await _analyticsService.GetDailySalesAsync(days ?? 7, cancellationToken);
            return Ok(dailySales);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("daily")]
    public async Task<ActionResult<DailySalesDto>> GetDaily([FromQuery] string date, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(date) || !DateTime.TryParse(date, out var parsedDate))
            {
                return BadRequest(new { error = "Data inválida. Use o formato YYYY-MM-DD" });
            }

            var dailySales = await _analyticsService.GetDailySalesByDateAsync(parsedDate, cancellationToken);
            return Ok(dailySales);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("range")]
    public async Task<ActionResult<DailySalesRangeResponseDto>> GetRange([FromQuery] string start, [FromQuery] string end, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(start) || !DateTime.TryParse(start, out var startDate))
            {
                return BadRequest(new { error = "Data inicial inválida. Use o formato YYYY-MM-DD" });
            }

            if (string.IsNullOrWhiteSpace(end) || !DateTime.TryParse(end, out var endDate))
            {
                return BadRequest(new { error = "Data final inválida. Use o formato YYYY-MM-DD" });
            }

            if (startDate > endDate)
            {
                return BadRequest(new { error = "Data inicial não pode ser maior que data final" });
            }

            var dailySales = await _analyticsService.GetDailySalesRangeAsync(startDate, endDate, cancellationToken);
            return Ok(dailySales);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }
}
