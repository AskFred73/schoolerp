namespace SchoolERP.Domain.Entities;

public class AuditLog
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string EntityName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string? UserId { get; set; }
    public string PayloadHash { get; set; } = string.Empty;
    public string? DetailsJson { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
