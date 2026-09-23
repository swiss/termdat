using Bk.Termdat.Business.Public.Api.Resource.Models;
using FluentValidation;

namespace Bk.Termdat.Business.Public.Api.Resource.Validators;

public class SearchParameterValidator : AbstractValidator<SearchParameter>
{
    public SearchParameterValidator()
    {
        RuleFor(p => p.MaxEntryCount)
            .GreaterThan(0)
            .LessThanOrEqualTo(1000);
    }
}
