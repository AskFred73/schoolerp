using Microsoft.EntityFrameworkCore;
using SchoolERP.Domain.Entities;

namespace SchoolERP.Infrastructure.Data;

public class SchoolErpDbContext : DbContext
{
    public SchoolErpDbContext(DbContextOptions<SchoolErpDbContext> options) : base(options) { }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<AcademicYear> AcademicYears => Set<AcademicYear>();
    public DbSet<AppUser> AppUsers => Set<AppUser>();
    public DbSet<SchoolClass> Classes => Set<SchoolClass>();
    public DbSet<House> Houses => Set<House>();
    public DbSet<TransportRoute> TransportRoutes => Set<TransportRoute>();
    public DbSet<Staff> Staff => Set<Staff>();
    public DbSet<Subject> Subjects => Set<Subject>();
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Guardian> Guardians => Set<Guardian>();
    public DbSet<StudentMedicalNote> StudentMedicalNotes => Set<StudentMedicalNote>();
    public DbSet<AttendanceDay> AttendanceDays => Set<AttendanceDay>();
    public DbSet<ExamResult> ExamResults => Set<ExamResult>();
    public DbSet<AppraisalNote> AppraisalNotes => Set<AppraisalNote>();
    public DbSet<FeeInvoice> FeeInvoices => Set<FeeInvoice>();
    public DbSet<FeePayment> FeePayments => Set<FeePayment>();
    public DbSet<TimetableSlot> TimetableSlots => Set<TimetableSlot>();
    public DbSet<StaffLeave> StaffLeaves => Set<StaffLeave>();
    public DbSet<ApprovalRequest> ApprovalRequests => Set<ApprovalRequest>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<DashboardSnapshot> DashboardSnapshots => Set<DashboardSnapshot>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("pgcrypto");

        modelBuilder.Entity<Tenant>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Code).HasMaxLength(32);
        });

        modelBuilder.Entity<Student>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.TenantId, x.AdmissionNo }).IsUnique();
            e.Property(x => x.AdmissionNo).HasMaxLength(32);
            e.Property(x => x.Section).HasMaxLength(4);
        });

        modelBuilder.Entity<FeeInvoice>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.InvoiceNumber).IsUnique();
            e.Property(x => x.InvoiceNumber).HasMaxLength(32);
        });

        modelBuilder.Entity<TimetableSlot>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.ClassId, x.DayOfWeek, x.PeriodNumber, x.AcademicYear }).IsUnique();
        });

        modelBuilder.Entity<AppUser>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.TenantId, x.Email }).IsUnique();
        });

        modelBuilder.Entity<AuditLog>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.PayloadHash).HasMaxLength(128);
        });

        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            if (entity.ClrType == typeof(AuditLog))
                continue;
        }
    }
}
