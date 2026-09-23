using Bk.Termdat.Business.Resource.Models;
using FluentValidation;

namespace Bk.Termdat.Business.Resource.Validators;

public class SearchFilterParameterValidator : AbstractValidator<SearchFilterParameter>
{
    public SearchFilterParameterValidator()
    {
        RuleFor(q => q.PageIndex).GreaterThanOrEqualTo(1);

        RuleFor(q => q.PageSize)
            .GreaterThanOrEqualTo(1)
            .LessThanOrEqualTo(5000);
    }
}
