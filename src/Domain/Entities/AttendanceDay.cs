using SchoolERP.Domain.Common;
using SchoolERP.Domain.Enums;

namespace SchoolERP.Domain.Entities;

public class AttendanceDay : ITenantEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid StudentId { get; set; }
    public Student? Student { get; set; }
    public DateOnly Date { get; set; }
    public AttendanceStatus Status { get; set; }
    public TimeOnly? CheckInTime { get; set; }
}
