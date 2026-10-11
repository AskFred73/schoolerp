using SchoolERP.Domain.Common;

namespace SchoolERP.Domain.Entities;

public class TimetableSlot : ITenantEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid ClassId { get; set; }
    public SchoolClass? Class { get; set; }
    public int DayOfWeek { get; set; }
    public int PeriodNumber { get; set; }
    public string SubjectCode { get; set; } = string.Empty;
    public Guid? TeacherId { get; set; }
    public Staff? Teacher { get; set; }
    public Guid? SubstituteTeacherId { get; set; }
    public Staff? SubstituteTeacher { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public string AcademicYear { get; set; } = "2026-27";
}
