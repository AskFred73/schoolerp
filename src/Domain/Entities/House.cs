using SchoolERP.Domain.Common;

namespace SchoolERP.Domain.Entities;

public class House : ITenantEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
}
