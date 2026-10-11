using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolERP.Infrastructure.Services;

namespace SchoolERP.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/analytics")]
public class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsService _analytics;
    private readonly ITenantContextAccessor _tenant;

    public AnalyticsController(IAnalyticsService analytics, ITenantContextAccessor tenant)
    {
        _analytics = analytics;
        _tenant = tenant;
    }

    [HttpGet("dashboard-summary")]
    public async Task<IActionResult> GetDashboardSummary(CancellationToken ct)
    {
        if (_tenant.TenantId is not Guid tenantId)
            return BadRequest("Tenant not resolved.");

        var result = await _analytics.GetDashboardSummaryAsync(tenantId, ct);
        return result is null ? NotFound() : Ok(result);
    }
}
