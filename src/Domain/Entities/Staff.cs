using SchoolERP.Domain.Common;

namespace SchoolERP.Domain.Entities;

public class Staff : ITenantEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? PrimarySubjectCode { get; set; }
    public bool IsActive { get; set; } = true;
}
