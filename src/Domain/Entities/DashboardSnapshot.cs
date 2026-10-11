using SchoolERP.Domain.Common;

namespace SchoolERP.Domain.Entities;

/// <summary>
/// Precomputed dashboard aggregates so local dev matches PRD KPIs without 3k+ student rows.
/// </summary>
public class DashboardSnapshot : ITenantEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public int TotalEnrollment { get; set; }
    public int MaleEnrollment { get; set; }
    public int FemaleEnrollment { get; set; }
    public decimal EnrollmentYoyChangePercent { get; set; }
    public decimal DailyAttendancePercent { get; set; }
    public int StaffActive { get; set; }
    public int StaffTotal { get; set; }
    public int ApprovedLeavesToday { get; set; }
    public int SubstitutionsAllocated { get; set; }
    public decimal Term1RevenueTarget { get; set; }
    public decimal Term1RevenueRealized { get; set; }
    public decimal Term1Overdue { get; set; }
    public decimal AnnualAssessment { get; set; }
    public decimal TotalRealized { get; set; }
    public decimal TotalOverdue { get; set; }
    public decimal CounterCollectionsToday { get; set; }
    public int DailyTransactionCount { get; set; }
}
