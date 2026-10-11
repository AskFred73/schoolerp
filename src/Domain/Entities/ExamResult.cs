using SchoolERP.Domain.Common;

namespace SchoolERP.Domain.Entities;

public class ExamResult : ITenantEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid StudentId { get; set; }
    public Student? Student { get; set; }
    public string ExamName { get; set; } = "Mid-Term";
    public decimal GpaPercent { get; set; }
    public int Rank { get; set; }
    public int ClassStrength { get; set; }
    public string MarksBreakdownJson { get; set; } = "{}";
}
