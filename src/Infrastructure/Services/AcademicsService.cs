using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SchoolERP.Domain.Services;
using SchoolERP.Infrastructure.Data;

namespace SchoolERP.Infrastructure.Services;

public record TimetableResponseDto(
    Guid ClassId,
    string ClassName,
    string AcademicYear,
    IReadOnlyList<TimetableSlotDto> Slots,
    IReadOnlyList<SubjectQuotaDto> SubjectQuotas,
    IReadOnlyList<TimetableConflictDto> Conflicts);

public record TimetableSlotDto(
    Guid Id,
    int DayOfWeek,
    int PeriodNumber,
    string SubjectCode,
    string? TeacherName,
    string? SubstituteTeacherName,
    string RoomNumber);

public record SubjectQuotaDto(string SubjectCode, int Scheduled, int Quota, bool IsCompliant);
public record TimetableConflictDto(string ConflictType, string Message, Guid? SlotId, int DayOfWeek, int PeriodNumber);

public record SubstitutionAllocateRequest(Guid TimetableSlotId, Guid SubstituteTeacherId, string? Reason);

public interface IAcademicsService
{
    Task<TimetableResponseDto?> GetTimetableAsync(Guid tenantId, Guid classId, CancellationToken ct = default);
    Task<bool> AllocateSubstitutionAsync(Guid tenantId, SubstitutionAllocateRequest request, CancellationToken ct = default);
}

public interface ISmsNotificationService
{
    Task SendSubstitutionNoticeAsync(string phone, string message, CancellationToken ct = default);
}

public class NoOpSmsNotificationService : ISmsNotificationService
{
    private readonly ILogger<NoOpSmsNotificationService> _logger;
    public NoOpSmsNotificationService(ILogger<NoOpSmsNotificationService> logger) => _logger = logger;

    public Task SendSubstitutionNoticeAsync(string phone, string message, CancellationToken ct = default)
    {
        _logger.LogInformation("SMS stub to {Phone}: {Message}", phone, message);
        return Task.CompletedTask;
    }
}

public class AcademicsService : IAcademicsService
{
    private readonly SchoolErpDbContext _db;
    private readonly ISmsNotificationService _sms;

    public AcademicsService(SchoolErpDbContext db, ISmsNotificationService sms)
    {
        _db = db;
        _sms = sms;
    }

    public async Task<TimetableResponseDto?> GetTimetableAsync(Guid tenantId, Guid classId, CancellationToken ct = default)
    {
        var cls = await _db.Classes.AsNoTracking().FirstOrDefaultAsync(c => c.Id == classId && c.TenantId == tenantId, ct);
        if (cls is null) return null;

        var slots = await _db.TimetableSlots.AsNoTracking()
            .Include(s => s.Teacher)
            .Include(s => s.SubstituteTeacher)
            .Where(s => s.TenantId == tenantId && s.ClassId == classId)
            .OrderBy(s => s.DayOfWeek).ThenBy(s => s.PeriodNumber)
            .ToListAsync(ct);

        var allSlots = await _db.TimetableSlots.AsNoTracking()
            .Where(s => s.TenantId == tenantId)
            .ToListAsync(ct);

        var conflicts = TimetableConflictDetector.Detect(allSlots, classId)
            .Select(c => new TimetableConflictDto(c.ConflictType, c.Message, c.SlotId, c.DayOfWeek, c.PeriodNumber))
            .ToList();

        var subjects = await _db.Subjects.AsNoTracking().Where(s => s.TenantId == tenantId).ToListAsync(ct);
        var quotas = subjects.Select(sub =>
        {
            var scheduled = sub.Code switch
            {
                "MTH-101" => slots.Count(s => s.SubjectCode == "MTH-101"),
                "SCI-201" => slots.Count(s => s.SubjectCode == "SCI-201"),
                "CCS-301" => slots.Count(s => s.SubjectCode == "CCS-301"),
                _ => slots.Count(s => s.SubjectCode == sub.Code)
            };
            return new SubjectQuotaDto(sub.Code, scheduled, sub.WeeklyPeriodQuota, scheduled >= sub.WeeklyPeriodQuota);
        }).ToList();

        return new TimetableResponseDto(
            classId,
            cls.Name,
            "2026-27",
            slots.Select(s => new TimetableSlotDto(
                s.Id,
                s.DayOfWeek,
                s.PeriodNumber,
                s.SubjectCode,
                s.Teacher is null ? null : $"{s.Teacher.FirstName} {s.Teacher.LastName}",
                s.SubstituteTeacher is null ? null : $"{s.SubstituteTeacher.FirstName} {s.SubstituteTeacher.LastName}",
                s.RoomNumber)).ToList(),
            quotas,
            conflicts);
    }

    public async Task<bool> AllocateSubstitutionAsync(Guid tenantId, SubstitutionAllocateRequest request, CancellationToken ct = default)
    {
        var slot = await _db.TimetableSlots
            .Include(s => s.Teacher)
            .FirstOrDefaultAsync(s => s.Id == request.TimetableSlotId && s.TenantId == tenantId, ct);
        if (slot is null) return false;

        var substitute = await _db.Staff.FirstOrDefaultAsync(
            s => s.Id == request.SubstituteTeacherId && s.TenantId == tenantId && s.IsActive, ct);
        if (substitute is null) return false;

        var clash = await _db.TimetableSlots.AnyAsync(s =>
            s.TenantId == tenantId &&
            s.DayOfWeek == slot.DayOfWeek &&
            s.PeriodNumber == slot.PeriodNumber &&
            s.TeacherId == substitute.Id &&
            s.Id != slot.Id, ct);
        if (clash) throw new InvalidOperationException("Substitute teacher has a conflicting slot.");

        slot.SubstituteTeacherId = substitute.Id;
        await _db.SaveChangesAsync(ct);

        await _sms.SendSubstitutionNoticeAsync(
            "+91-0000000000",
            $"Substitution allocated: {substitute.FirstName} {substitute.LastName} for {slot.SubjectCode} (Day {slot.DayOfWeek} P{slot.PeriodNumber}).",
            ct);

        return true;
    }
}
