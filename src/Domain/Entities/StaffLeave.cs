using SchoolERP.Domain.Common;

namespace SchoolERP.Domain.Entities;

public class StaffLeave : ITenantEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid StaffId { get; set; }
    public Staff? Staff { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public bool IsApproved { get; set; }
    public string Reason { get; set; } = string.Empty;
}
