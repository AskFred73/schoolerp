using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolERP.Api.Validation;
using SchoolERP.Infrastructure.Services;

namespace SchoolERP.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/academics")]
public class AcademicsController : ControllerBase
{
    private readonly IAcademicsService _academics;
    private readonly ITenantContextAccessor _tenant;

    public AcademicsController(IAcademicsService academics, ITenantContextAccessor tenant)
    {
        _academics = academics;
        _tenant = tenant;
    }

    [HttpGet("timetable/{classId:guid}")]
    public async Task<IActionResult> GetTimetable(Guid classId, CancellationToken ct)
    {
        if (_tenant.TenantId is not Guid tenantId) return BadRequest();
        var result = await _academics.GetTimetableAsync(tenantId, classId, ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost("substitutions/allocate")]
    [Authorize(Roles = "AcademicCoordinator,Principal")]
    public async Task<IActionResult> AllocateSubstitution([FromBody] SubstitutionAllocateRequest request, CancellationToken ct)
    {
        if (_tenant.TenantId is not Guid tenantId) return BadRequest();
        try
        {
            var ok = await _academics.AllocateSubstitutionAsync(tenantId, request, ct);
            return ok ? Ok(new { success = true }) : NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
