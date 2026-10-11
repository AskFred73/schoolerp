using SchoolERP.Domain.Common;
using SchoolERP.Domain.Enums;

namespace SchoolERP.Domain.Entities;

public class FeePayment : ITenantEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid FeeInvoiceId { get; set; }
    public FeeInvoice? FeeInvoice { get; set; }
    public Guid? StudentId { get; set; }
    public PaymentChannel Channel { get; set; }
    public decimal Amount { get; set; }
    public string ReceiptNumber { get; set; } = string.Empty;
    public DateTime CollectedAt { get; set; } = DateTime.UtcNow;
    public string? CollectedByUserId { get; set; }
}
