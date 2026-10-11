using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using SchoolERP.Domain.Entities;

namespace SchoolERP.Infrastructure.Data;

public class AuditSaveChangesInterceptor : SaveChangesInterceptor
{
    private readonly Func<Guid?> _tenantIdProvider;
    private readonly Func<string?> _userIdProvider;

    public AuditSaveChangesInterceptor(Func<Guid?> tenantIdProvider, Func<string?> userIdProvider)
    {
        _tenantIdProvider = tenantIdProvider;
        _userIdProvider = userIdProvider;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is SchoolErpDbContext ctx)
            AppendAuditEntries(ctx);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void AppendAuditEntries(SchoolErpDbContext context)
    {
        var tenantId = _tenantIdProvider() ?? Guid.Empty;
        var userId = _userIdProvider();

        var entries = context.ChangeTracker.Entries()
            .Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .Where(e => e.Entity is not AuditLog)
            .ToList();

        foreach (var entry in entries)
        {
            var entityName = entry.Entity.GetType().Name;
            var action = entry.State.ToString();
            var payload = JsonSerializer.Serialize(entry.Properties.ToDictionary(p => p.Metadata.Name, p => p.CurrentValue));
            var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(payload)));

            context.AuditLogs.Add(new AuditLog
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                EntityName = entityName,
                Action = action,
                UserId = userId,
                PayloadHash = hash,
                DetailsJson = payload.Length > 4000 ? payload[..4000] : payload
            });
        }
    }
}
