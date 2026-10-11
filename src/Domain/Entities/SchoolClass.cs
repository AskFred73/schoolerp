using SchoolERP.Domain.Common;
using SchoolERP.Domain.Enums;

namespace SchoolERP.Domain.Entities;

public class SchoolClass : ITenantEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int GradeLevel { get; set; }
    public AcademicWing Wing { get; set; }
    public ICollection<Student> Students { get; set; } = new List<Student>();
}
