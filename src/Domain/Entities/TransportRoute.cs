using SchoolERP.Domain.Common;

namespace SchoolERP.Domain.Entities;

public class TransportRoute : ITenantEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public int RouteNumber { get; set; }
    public string Description { get; set; } = string.Empty;
}
