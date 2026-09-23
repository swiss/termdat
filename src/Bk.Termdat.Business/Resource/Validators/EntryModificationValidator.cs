using System.Collections.Generic;
using System.Linq;
using Bk.Termdat.Business.Resource.Models;
using Bk.Termdat.Common.Resources;
using FluentValidation;

namespace Bk.Termdat.Business.Resource.Validators;

public abstract class EntryModificationValidator<T> : AbstractValidator<T>
    where T : IEntryModification
{
    protected EntryModificationValidator()
    {
        RuleFor(ce => ce.Comment)
            .MaximumLength(4000)
            .WithName(_ => BackendTexts.EntryComment);

        RuleFor(ce => ce.SubjectIds)
            .NotNull()
            .NotEmpty()
            .ForEach(subjectIds => subjectIds.Must(id => id != default))
            .WithName(_ => BackendTexts.EntrySubject);

        RuleFor(ce => ce.DomainIds)
            .ForEach(domainIds => domainIds.Must(id => id != default))
            .WithName(_ => BackendTexts.EntryDomain);

        RuleFor(ce => ce.LenochClassificationIds)
            .ForEach(lenochClassificationIds => lenochClassificationIds.Must(id => id != default))
            .WithName(_ => BackendTexts.EntryLenochClassification);

        RuleFor(ce => ce.CollectionId)
            .NotEmpty()
            .WithName(_ => BackendTexts.EntryCollection);

        RuleFor(ce => ce.OfficeId)
            .NotEmpty()
            .WithName(_ => BackendTexts.EntryOffice);

        RuleFor(ce => ce.LanguageDetails)
            .NotEmpty();

        RuleForEach(ce => ce.LanguageDetails)
            .SetValidator(new EntryLanguageDetailValidator());

        RuleFor(ce => ce.References)
            .Must(r => !HasDuplicates(r))
            .WithMessage(BackendTexts.ReferencesMustBeUnique);
    }

    protected bool HasDuplicates(IEnumerable<EntryModificationReference> refs)
    {
        if (refs != null)
        {
            return refs.GroupBy(r => new { r.Type, r.EntryId })
                .Any(g => g.Count() > 1);
        }

        return false;
    }
}
