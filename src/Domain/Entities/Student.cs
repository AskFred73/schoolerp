using SchoolERP.Domain.Common;
using SchoolERP.Domain.Enums;

namespace SchoolERP.Domain.Entities;

public class Student : ITenantEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string AdmissionNo { get; set; } = string.Empty;
    public int RollNo { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public Guid ClassId { get; set; }
    public SchoolClass? Class { get; set; }
    public string Section { get; set; } = string.Empty;
    public Guid? HouseId { get; set; }
    public House? House { get; set; }
    public string? CbseRegNo { get; set; }
    public bool AadhaarLinked { get; set; }
    public string? AadhaarLastFour { get; set; }
    public Guid? BusRouteId { get; set; }
    public TransportRoute? BusRoute { get; set; }
    public StudentStatus Status { get; set; } = StudentStatus.ACTIVE;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Guardian> Guardians { get; set; } = new List<Guardian>();
    public ICollection<StudentMedicalNote> MedicalNotes { get; set; } = new List<StudentMedicalNote>();
    public ICollection<AttendanceDay> AttendanceDays { get; set; } = new List<AttendanceDay>();
    public ICollection<FeeInvoice> FeeInvoices { get; set; } = new List<FeeInvoice>();
    public ICollection<ExamResult> ExamResults { get; set; } = new List<ExamResult>();
    public ICollection<AppraisalNote> AppraisalNotes { get; set; } = new List<AppraisalNote>();
}
