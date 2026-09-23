using System.Linq;
using Bk.Termdat.Business.Resource.Models;
using FluentValidation;

namespace Bk.Termdat.Business.Resource.Validators;

public class GetDetailQueryValidator : AbstractValidator<GetDetailQuery>
{
    public GetDetailQueryValidator()
    {
        RuleForEach(q => q.SourceLanguageIds)
            .NotEmpty()
            .When(q => q.SourceLanguageIds != null && q.SourceLanguageIds.Any());

        RuleForEach(q => q.TargetLanguageIds)
            .NotEmpty()
            .When(q => q.TargetLanguageIds != null && q.TargetLanguageIds.Any());
    }
}
