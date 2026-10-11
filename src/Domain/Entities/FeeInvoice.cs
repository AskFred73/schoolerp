using SchoolERP.Domain.Common;
using SchoolERP.Domain.Enums;

namespace SchoolERP.Domain.Entities;

public class FeeInvoice : ITenantEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public Guid StudentId { get; set; }
    public Student? Student { get; set; }
    public string AcademicYear { get; set; } = "2026-27";
    public int Term { get; set; }
    public decimal TuitionFee { get; set; }
    public decimal LabSmartboardFee { get; set; }
    public decimal TransportFee { get; set; }
    public decimal LateFine { get; set; }
    public decimal WaivedFine { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal AmountPaid { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public DateOnly DueDate { get; set; }

    public ICollection<FeePayment> Payments { get; set; } = new List<FeePayment>();
}
