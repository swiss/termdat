using Bk.Termdat.Business.Resource.Models;
using FluentValidation;

namespace Bk.Termdat.Business.Resource.Validators;

public class RecommendQueryValidator : AbstractValidator<RecommendQuery>
{
    public RecommendQueryValidator()
    {
        RuleFor(q => q.Phrase)
            .NotNull()
            .MinimumLength(3);
    }
}
