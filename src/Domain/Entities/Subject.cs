using SchoolERP.Domain.Common;

namespace SchoolERP.Domain.Entities;

public class Subject : ITenantEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int WeeklyPeriodQuota { get; set; }
}
