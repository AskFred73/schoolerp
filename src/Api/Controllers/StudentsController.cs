using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolERP.Infrastructure.Services;

namespace SchoolERP.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/students")]
public class StudentsController : ControllerBase
{
    private readonly IStudent360Service _students;
    private readonly ITenantContextAccessor _tenant;

    public StudentsController(IStudent360Service students, ITenantContextAccessor tenant)
    {
        _students = students;
        _tenant = tenant;
    }

    [HttpGet("{admissionNo}/360")]
    public async Task<IActionResult> Get360(string admissionNo, CancellationToken ct)
    {
        if (_tenant.TenantId is not Guid tenantId)
            return BadRequest("Tenant not resolved.");

        var result = await _students.GetByAdmissionNoAsync(tenantId, admissionNo, ct);
        return result is null ? NotFound() : Ok(result);
    }
}
