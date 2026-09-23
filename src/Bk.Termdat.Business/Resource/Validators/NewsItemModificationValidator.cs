using Bk.Termdat.Business.Resource.Models;
using FluentValidation;

namespace Bk.Termdat.Business.Resource.Validators;

public abstract class NewsItemModificationValidator<T> : AbstractValidator<T>
    where T : INewsItemModification
{
    protected NewsItemModificationValidator()
    {
        RuleFor(p => p.TitleDe).NotEmpty().MaximumLength(1000);
        RuleFor(p => p.TitleFr).NotEmpty().MaximumLength(1000);
        RuleFor(p => p.TitleEn).NotEmpty().MaximumLength(1000);
        RuleFor(p => p.TitleIt).NotEmpty().MaximumLength(1000);
        RuleFor(p => p.TitleRm).NotEmpty().MaximumLength(1000);

        RuleFor(p => p.TextDe).NotEmpty().MaximumLength(4000);
        RuleFor(p => p.TextFr).NotEmpty().MaximumLength(4000);
        RuleFor(p => p.TextEn).NotEmpty().MaximumLength(4000);
        RuleFor(p => p.TextIt).NotEmpty().MaximumLength(4000);
        RuleFor(p => p.TextRm).NotEmpty().MaximumLength(4000);

        RuleFor(p => p.ValidFrom).NotEmpty();
        RuleFor(p => p.ValidTo).NotEmpty();
    }
}
