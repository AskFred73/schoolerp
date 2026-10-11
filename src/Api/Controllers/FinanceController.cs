using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolERP.Infrastructure.Services;

namespace SchoolERP.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/finance")]
public class FinanceController : ControllerBase
{
    private readonly IFinanceService _finance;
    private readonly ITenantContextAccessor _tenant;

    public FinanceController(IFinanceService finance, ITenantContextAccessor tenant)
    {
        _finance = finance;
        _tenant = tenant;
    }

    [HttpGet("fees/summary")]
    public async Task<IActionResult> GetSummary(CancellationToken ct)
    {
        if (_tenant.TenantId is not Guid tenantId) return BadRequest();
        var result = await _finance.GetSummaryAsync(tenantId, ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("fees/invoices")]
    public async Task<IActionResult> ListInvoices([FromQuery] string? wing, [FromQuery] string? status, CancellationToken ct)
    {
        if (_tenant.TenantId is not Guid tenantId) return BadRequest();
        var list = await _finance.ListInvoicesAsync(tenantId, wing, status, ct);
        return Ok(list);
    }

    [HttpPost("pos/collect")]
    [Authorize(Roles = "Accountant,Principal")]
    public async Task<IActionResult> Collect([FromBody] PosCollectRequest request, CancellationToken ct)
    {
        if (_tenant.TenantId is not Guid tenantId) return BadRequest();

        if (request.WaiveLateFine && _tenant.UserRole is not ("Principal" or "Accountant"))
            return Forbid();

        try
        {
            var receipt = await _finance.CollectAsync(tenantId, request, _tenant.UserId, ct);
            return receipt is null ? NotFound() : Ok(receipt);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
