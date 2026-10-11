using Microsoft.EntityFrameworkCore;
using SchoolERP.Infrastructure.Data;

namespace SchoolERP.Infrastructure.Services;

public record Student360Dto(
    StudentHeaderDto Header,
    IReadOnlyList<GuardianDto> Guardians,
    IReadOnlyList<MedicalNoteDto> MedicalNotes,
    AttendanceSummaryDto Attendance,
    AcademicRecordDto Academics,
    FinancialLedgerDto Finance);

public record StudentHeaderDto(
    string AdmissionNo,
    int RollNo,
    string FullName,
    string ClassSection,
    string? CbseRegNo,
    bool AadhaarLinked,
    string? AadhaarMasked,
    string? House,
    string? BusRoute);

public record GuardianDto(string Relation, string FullName, string? Occupation, string Phone, bool SmsEnabled, bool IsPrimary, string? Address, bool AddressVerified);
public record MedicalNoteDto(string Note, DateTime RecordedAt);
public record AttendanceSummaryDto(int TotalDays, int Present, int Absent, int Leave, IReadOnlyList<AttendanceDayDto> Days);
public record AttendanceDayDto(string Date, string Status);
public record AcademicRecordDto(string ExamName, decimal GpaPercent, int Rank, int ClassStrength, string MarksBreakdownJson, string? AppraisalNote);
public record FinancialLedgerDto(decimal OutstandingBalance, IReadOnlyList<InvoiceDto> Invoices);
public record InvoiceDto(string InvoiceNumber, int Term, decimal Total, decimal Paid, string Status, DateOnly DueDate);

public interface IStudent360Service
{
    Task<Student360Dto?> GetByAdmissionNoAsync(Guid tenantId, string admissionNo, CancellationToken ct = default);
}

public class Student360Service : IStudent360Service
{
    private readonly SchoolErpDbContext _db;

    public Student360Service(SchoolErpDbContext db) => _db = db;

    public async Task<Student360Dto?> GetByAdmissionNoAsync(Guid tenantId, string admissionNo, CancellationToken ct = default)
    {
        var student = await _db.Students.AsNoTracking()
            .Include(s => s.Class)
            .Include(s => s.House)
            .Include(s => s.BusRoute)
            .FirstOrDefaultAsync(s => s.TenantId == tenantId && s.AdmissionNo == admissionNo, ct);
        if (student is null) return null;

        var guardians = await _db.Guardians.AsNoTracking()
            .Where(g => g.StudentId == student.Id)
            .Select(g => new GuardianDto(
                g.Relation.ToString(),
                g.FullName,
                g.Occupation,
                g.Phone,
                g.SmsEnabled,
                g.IsPrimary,
                g.AddressLine,
                g.AddressVerified))
            .ToListAsync(ct);

        var medical = await _db.StudentMedicalNotes.AsNoTracking()
            .Where(m => m.StudentId == student.Id)
            .Select(m => new MedicalNoteDto(m.Note, m.RecordedAt))
            .ToListAsync(ct);

        var days = await _db.AttendanceDays.AsNoTracking()
            .Where(a => a.StudentId == student.Id)
            .OrderBy(a => a.Date)
            .ToListAsync(ct);

        var present = days.Count(d => d.Status == Domain.Enums.AttendanceStatus.Present);
        var absent = days.Count(d => d.Status == Domain.Enums.AttendanceStatus.Absent);
        var leave = days.Count(d => d.Status == Domain.Enums.AttendanceStatus.Leave);

        var exam = await _db.ExamResults.AsNoTracking()
            .Where(e => e.StudentId == student.Id)
            .OrderByDescending(e => e.GpaPercent)
            .FirstOrDefaultAsync(ct);

        var appraisal = await _db.AppraisalNotes.AsNoTracking()
            .Where(a => a.StudentId == student.Id)
            .OrderByDescending(a => a.RecordedAt)
            .FirstOrDefaultAsync(ct);

        var invoices = await _db.FeeInvoices.AsNoTracking()
            .Where(f => f.StudentId == student.Id)
            .OrderByDescending(f => f.Term)
            .ToListAsync(ct);

        var outstanding = invoices.Sum(i => i.TotalAmount - i.AmountPaid);

        var aadhaarMasked = student.AadhaarLinked && !string.IsNullOrEmpty(student.AadhaarLastFour)
            ? $"XXXX-XXXX-{student.AadhaarLastFour}"
            : null;

        return new Student360Dto(
            new StudentHeaderDto(
                student.AdmissionNo,
                student.RollNo,
                $"{student.FirstName} {student.LastName}",
                $"{student.Class?.Name}-{student.Section}",
                student.CbseRegNo,
                student.AadhaarLinked,
                aadhaarMasked,
                student.House?.Name,
                student.BusRoute is null ? null : $"#{student.BusRoute.RouteNumber}"),
            guardians,
            medical,
            new AttendanceSummaryDto(
                days.Count,
                present,
                absent,
                leave,
                days.Select(d => new AttendanceDayDto(d.Date.ToString("yyyy-MM-dd"), d.Status.ToString())).ToList()),
            new AcademicRecordDto(
                exam?.ExamName ?? "N/A",
                exam?.GpaPercent ?? 0,
                exam?.Rank ?? 0,
                exam?.ClassStrength ?? 0,
                exam?.MarksBreakdownJson ?? "{}",
                appraisal?.Note),
            new FinancialLedgerDto(
                outstanding,
                invoices.Select(i => new InvoiceDto(
                    i.InvoiceNumber,
                    i.Term,
                    i.TotalAmount,
                    i.AmountPaid,
                    i.PaymentStatus.ToString(),
                    i.DueDate)).ToList()));
    }
}
