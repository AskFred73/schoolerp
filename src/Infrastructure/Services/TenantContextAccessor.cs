namespace SchoolERP.Infrastructure.Services;

public interface ITenantContextAccessor
{
    Guid? TenantId { get; set; }
    string? UserId { get; set; }
    string? UserRole { get; set; }
}

public class TenantContextAccessor : ITenantContextAccessor
{
    public Guid? TenantId { get; set; }
    public string? UserId { get; set; }
    public string? UserRole { get; set; }
}
