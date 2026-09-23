using Bk.Termdat.Business.Resource.Models;
using Bk.Termdat.Common.Resources;
using FluentValidation;

namespace Bk.Termdat.Business.Resource.Validators;

public class PictureValidator : AbstractValidator<PictureDetail>
{
    public PictureValidator()
    {
        RuleFor(p => p)
            .Must(p => !string.IsNullOrEmpty(p.DescriptionDe) || !string.IsNullOrEmpty(p.DescriptionEn) ||
                       !string.IsNullOrEmpty(p.DescriptionFr) || !string.IsNullOrEmpty(p.DescriptionIt) || !string.IsNullOrEmpty(p.DescriptionRm))
            .WithMessage(
                _ => BackendTexts.PictureMinimalFields);

        RuleFor(p => p.DescriptionDe)
            .MaximumLength(4000)
            .WithName(_ => BackendTexts.PictureDescriptionDe);

        RuleFor(p => p.DescriptionEn)
            .MaximumLength(4000)
            .WithName(_ => BackendTexts.PictureDescriptionEn);

        RuleFor(p => p.DescriptionFr)
            .MaximumLength(4000)
            .WithName(_ => BackendTexts.PictureDescriptionFr);

        RuleFor(p => p.DescriptionIt)
            .MaximumLength(4000)
            .WithName(_ => BackendTexts.PictureDescriptionIt);

        RuleFor(p => p.DescriptionRm)
            .MaximumLength(4000)
            .WithName(_ => BackendTexts.PictureDescriptionRm);

        RuleFor(p => p.SourceDe)
            .MaximumLength(4000)
            .WithName(_ => BackendTexts.PictureSourceDe);

        RuleFor(p => p.SourceEn)
            .MaximumLength(4000)
            .WithName(_ => BackendTexts.PictureSourceEn);

        RuleFor(p => p.SourceFr)
            .MaximumLength(4000)
            .WithName(_ => BackendTexts.PictureSourceFr);

        RuleFor(p => p.SourceIt)
            .MaximumLength(4000)
            .WithName(_ => BackendTexts.PictureSourceIt);

        RuleFor(p => p.SourceRm)
            .MaximumLength(4000)
            .WithName(_ => BackendTexts.PictureSourceRm);
    }
}
