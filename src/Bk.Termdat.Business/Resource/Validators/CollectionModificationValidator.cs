using Bk.Termdat.Business.Resource.Models;
using FluentValidation;

namespace Bk.Termdat.Business.Resource.Validators;

public abstract class CollectionModificationValidator<T> : AbstractValidator<T> where T : ICollectionModification
{
    protected CollectionModificationValidator()
    {
        RuleFor(p => p.Code).NotNull().MaximumLength(10);
        RuleFor(p => p.DescriptionDe).NotNull().NotEmpty().MaximumLength(1000);
        RuleFor(p => p.DescriptionFr).NotNull().NotEmpty().MaximumLength(1000);
        RuleFor(p => p.DescriptionEn).NotNull().NotEmpty().MaximumLength(1000);
        RuleFor(p => p.DescriptionIt).NotNull().NotEmpty().MaximumLength(1000);
        RuleFor(p => p.DescriptionRm).NotNull().NotEmpty().MaximumLength(1000);
        RuleFor(p => p.Documentation).MaximumLength(1000);
        RuleFor(p => p.Description).MaximumLength(1000);
    }
}
