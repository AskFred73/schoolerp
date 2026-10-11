using SchoolERP.Domain.Common;
using SchoolERP.Domain.Enums;

namespace SchoolERP.Domain.Entities;

public class ApprovalRequest : ITenantEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public ApprovalType Type { get; set; }
    public ApprovalStatus Status { get; set; } = ApprovalStatus.Pending;
    public int Tier { get; set; } = 1;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
