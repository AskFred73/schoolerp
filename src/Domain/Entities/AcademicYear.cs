using SchoolERP.Domain.Common;

namespace SchoolERP.Domain.Entities;

public class AcademicYear : ITenantEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Label { get; set; } = "2026-27";
    public bool IsCurrent { get; set; }
}
