using SchoolERP.Domain.Common;

namespace SchoolERP.Domain.Entities;

public class AppraisalNote : ITenantEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid StudentId { get; set; }
    public Student? Student { get; set; }
    public string Note { get; set; } = string.Empty;
    public string AuthorName { get; set; } = string.Empty;
    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
}
