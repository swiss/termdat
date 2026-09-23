using Bk.Termdat.Business.Resource.Models;
using FluentValidation;

namespace Bk.Termdat.Business.Resource.Validators;

public abstract class OfficeModificationValidator<T> : AbstractValidator<T> where T : IOfficeModification
{
    protected OfficeModificationValidator()
    {
        RuleFor(o => o.Code).NotNull().MaximumLength(5);
        RuleFor(o => o.DescriptionDe).NotNull().NotEmpty().MaximumLength(1000);
        RuleFor(o => o.DescriptionFr).NotNull().NotEmpty().MaximumLength(1000);
        RuleFor(o => o.DescriptionIt).NotNull().NotEmpty().MaximumLength(1000);
        RuleFor(o => o.DescriptionRm).NotNull().NotEmpty().MaximumLength(1000);
        RuleFor(o => o.DescriptionEn).NotNull().NotEmpty().MaximumLength(1000);
    }
}
