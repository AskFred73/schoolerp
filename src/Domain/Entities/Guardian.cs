using SchoolERP.Domain.Common;
using SchoolERP.Domain.Enums;

namespace SchoolERP.Domain.Entities;

public class Guardian : ITenantEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid StudentId { get; set; }
    public Student? Student { get; set; }
    public GuardianRelation Relation { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Occupation { get; set; }
    public string Phone { get; set; } = string.Empty;
    public bool SmsEnabled { get; set; }
    public bool IsPrimary { get; set; }
    public string? AddressLine { get; set; }
    public bool AddressVerified { get; set; }
}
