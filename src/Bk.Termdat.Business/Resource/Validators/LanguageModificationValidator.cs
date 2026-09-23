using Bk.Termdat.Business.Resource.Models;
using FluentValidation;

namespace Bk.Termdat.Business.Resource.Validators;

public abstract class LanguageModificationValidator<T> : AbstractValidator<T> where T : ILanguageModification
{
    protected LanguageModificationValidator()
    {
        RuleFor(p => p.IsoCode).NotNull().MaximumLength(2);
        RuleFor(p => p.DescriptionDe).NotNull().NotEmpty().MaximumLength(1000);
        RuleFor(p => p.DescriptionFr).NotNull().NotEmpty().MaximumLength(1000);
        RuleFor(p => p.DescriptionEn).NotNull().NotEmpty().MaximumLength(1000);
        RuleFor(p => p.DescriptionIt).NotNull().NotEmpty().MaximumLength(1000);
        RuleFor(p => p.DescriptionRm).NotNull().NotEmpty().MaximumLength(1000);
    }
}
