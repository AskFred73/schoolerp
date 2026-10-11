using Microsoft.EntityFrameworkCore;
using SchoolERP.Domain.Enums;
using SchoolERP.Domain.Services;
using SchoolERP.Infrastructure.Data;

namespace SchoolERP.Infrastructure.Services;

public record PosCollectRequest(
    string AdmissionNo,
    IReadOnlyList<Guid> InvoiceIds,
    PaymentChannel Channel,
    decimal Amount,
    bool WaiveLateFine,
    string? AuthorizedByUserId);

public record PosCollectReceiptDto(
    string ReceiptNumber,
    string StudentAdmissionNo,
    decimal AmountPaid,
    string Channel,
    IReadOnlyList<string> InvoiceNumbers,
    decimal RemainingBalance,
    DateTime CollectedAt,
    string ThermalLine);

public record FeesSummaryDto(
    decimal AnnualAssessment,
    decimal TotalRealized,
    decimal TotalOverdue,
    decimal CounterCollectionsToday);

public record FeeInvoiceListItemDto(
    Guid Id,
    string InvoiceNumber,
    string AdmissionNo,
    string StudentName,
    string ClassSection,
    string Wing,
    decimal TotalAmount,
    decimal AmountPaid,
    string PaymentStatus,
    DateOnly DueDate);

public interface IFinanceService
{
    Task<FeesSummaryDto?> GetSummaryAsync(Guid tenantId, CancellationToken ct = default);
    Task<IReadOnlyList<FeeInvoiceListItemDto>> ListInvoicesAsync(Guid tenantId, string? wing, string? status, CancellationToken ct = default);
    Task<PosCollectReceiptDto?> CollectAsync(Guid tenantId, PosCollectRequest request, string? collectedByUserId, CancellationToken ct = default);
}

public class FinanceService : IFinanceService
{
    private readonly SchoolErpDbContext _db;

    public FinanceService(SchoolErpDbContext db) => _db = db;

    public async Task<FeesSummaryDto?> GetSummaryAsync(Guid tenantId, CancellationToken ct = default)
    {
        var snap = await _db.DashboardSnapshots.AsNoTracking()
            .FirstOrDefaultAsync(s => s.TenantId == tenantId, ct);
        return snap is null
            ? null
            : new FeesSummaryDto(snap.AnnualAssessment, snap.TotalRealized, snap.TotalOverdue, snap.CounterCollectionsToday);
    }

    public async Task<IReadOnlyList<FeeInvoiceListItemDto>> ListInvoicesAsync(Guid tenantId, string? wing, string? status, CancellationToken ct = default)
    {
        var query = from inv in _db.FeeInvoices.AsNoTracking()
                    join st in _db.Students.AsNoTracking() on inv.StudentId equals st.Id
                    join cl in _db.Classes.AsNoTracking() on st.ClassId equals cl.Id
                    where inv.TenantId == tenantId
                    select new { inv, st, cl };

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(x => x.inv.PaymentStatus.ToString() == status);

        if (!string.IsNullOrWhiteSpace(wing))
            query = query.Where(x => x.cl.Wing.ToString() == wing);

        return await query
            .OrderBy(x => x.inv.DueDate)
            .Select(x => new FeeInvoiceListItemDto(
                x.inv.Id,
                x.inv.InvoiceNumber,
                x.st.AdmissionNo,
                x.st.FirstName + " " + x.st.LastName,
                x.cl.Name + "-" + x.st.Section,
                x.cl.Wing.ToString(),
                x.inv.TotalAmount,
                x.inv.AmountPaid,
                x.inv.PaymentStatus.ToString(),
                x.inv.DueDate))
            .Take(200)
            .ToListAsync(ct);
    }

    public async Task<PosCollectReceiptDto?> CollectAsync(Guid tenantId, PosCollectRequest request, string? collectedByUserId, CancellationToken ct = default)
    {
        var student = await _db.Students.FirstOrDefaultAsync(
            s => s.TenantId == tenantId && s.AdmissionNo == request.AdmissionNo, ct);
        if (student is null) return null;

        var invoices = await _db.FeeInvoices
            .Where(i => i.TenantId == tenantId && request.InvoiceIds.Contains(i.Id) && i.StudentId == student.Id)
            .ToListAsync(ct);
        if (invoices.Count == 0) return null;

        if (request.WaiveLateFine)
        {
            foreach (var inv in invoices)
            {
                inv.WaivedFine = inv.LateFine;
                inv.LateFine = 0;
                inv.TotalAmount = inv.TuitionFee + inv.LabSmartboardFee + inv.TransportFee + inv.LateFine - inv.WaivedFine;
            }
        }

        var remainingBefore = invoices.Sum(i => i.TotalAmount - i.AmountPaid);
        if (request.Amount <= 0 || request.Amount > remainingBefore)
            throw new InvalidOperationException("Invalid collection amount.");

        var amountLeft = request.Amount;
        foreach (var inv in invoices.OrderBy(i => i.DueDate))
        {
            var due = inv.TotalAmount - inv.AmountPaid;
            var apply = Math.Min(due, amountLeft);
            inv.AmountPaid += apply;
            FeeStatusCalculator.ApplyToInvoice(inv);
            amountLeft -= apply;
            if (amountLeft <= 0) break;
        }

        var receiptNo = $"RCP-{DateTime.UtcNow:yyyy}-{Random.Shared.Next(10000, 99999)}";
        _db.FeePayments.Add(new Domain.Entities.FeePayment
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            FeeInvoiceId = invoices[0].Id,
            StudentId = student.Id,
            Channel = request.Channel,
            Amount = request.Amount,
            ReceiptNumber = receiptNo,
            CollectedByUserId = collectedByUserId,
            CollectedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync(ct);

        var remaining = await _db.FeeInvoices.AsNoTracking()
            .Where(i => i.StudentId == student.Id)
            .SumAsync(i => i.TotalAmount - i.AmountPaid, ct);

        var line = $"SPG ERP | {student.AdmissionNo} | ₹{request.Amount:N2} | {request.Channel} | {receiptNo}";

        return new PosCollectReceiptDto(
            receiptNo,
            student.AdmissionNo,
            request.Amount,
            request.Channel.ToString(),
            invoices.Select(i => i.InvoiceNumber).ToList(),
            remaining,
            DateTime.UtcNow,
            line);
    }
}
