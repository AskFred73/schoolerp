using FluentValidation;
using SchoolERP.Infrastructure.Services;

namespace SchoolERP.Api.Validation;

public class PosCollectRequestValidator : AbstractValidator<PosCollectRequest>
{
    public PosCollectRequestValidator()
    {
        RuleFor(x => x.AdmissionNo).NotEmpty().MaximumLength(32);
        RuleFor(x => x.InvoiceIds).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.Channel).IsInEnum();
    }
}

public class SubstitutionAllocateRequestValidator : AbstractValidator<SubstitutionAllocateRequest>
{
    public SubstitutionAllocateRequestValidator()
    {
        RuleFor(x => x.TimetableSlotId).NotEmpty();
        RuleFor(x => x.SubstituteTeacherId).NotEmpty();
    }
}
