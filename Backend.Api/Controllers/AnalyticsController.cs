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
}
