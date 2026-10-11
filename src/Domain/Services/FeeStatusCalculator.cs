using SchoolERP.Domain.Entities;
using SchoolERP.Domain.Enums;

namespace SchoolERP.Domain.Services;

public static class FeeStatusCalculator
{
    public static PaymentStatus Compute(decimal totalAmount, decimal amountPaid, DateOnly dueDate, DateOnly? asOf = null)
    {
        var today = asOf ?? DateOnly.FromDateTime(DateTime.UtcNow);
        if (amountPaid >= totalAmount)
            return PaymentStatus.PAID;
        if (today > dueDate)
            return PaymentStatus.OVERDUE;
        return PaymentStatus.PARTIAL;
    }

    public static void ApplyToInvoice(FeeInvoice invoice, DateOnly? asOf = null)
    {
        invoice.PaymentStatus = Compute(invoice.TotalAmount, invoice.AmountPaid, invoice.DueDate, asOf);
    }
}
