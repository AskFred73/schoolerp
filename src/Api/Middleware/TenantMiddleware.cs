using SchoolERP.Infrastructure;
using SchoolERP.Infrastructure.Services;

namespace SchoolERP.Api.Middleware;

public class TenantMiddleware
{
    private readonly RequestDelegate _next;

    public TenantMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context, ITenantContextAccessor tenantAccessor)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var tenantClaim = context.User.FindFirst("tenant_id")?.Value;
            if (Guid.TryParse(tenantClaim, out var tenantId))
                tenantAccessor.TenantId = tenantId;
            tenantAccessor.UserId = context.User.FindFirst("sub")?.Value;
            tenantAccessor.UserRole = context.User.FindFirst("role")?.Value;
        }

        if (tenantAccessor.TenantId is null &&
            context.Request.Headers.TryGetValue("X-Tenant-Id", out var header) &&
            Guid.TryParse(header.ToString(), out var headerTenant))
        {
            tenantAccessor.TenantId = headerTenant;
        }

        if (tenantAccessor.TenantId is null &&
            !context.Request.Path.StartsWithSegments("/api/v1/auth") &&
            !context.Request.Path.StartsWithSegments("/swagger"))
        {
            tenantAccessor.TenantId = DemoIds.TenantId;
        }

        await _next(context);
    }
}
