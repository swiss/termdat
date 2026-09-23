using Bk.Termdat.Business.Resource.Models;
using Bk.Termdat.Business.Services.Interfaces;
using FluentValidation;

namespace Bk.Termdat.Business.Resource.Validators;

public class SearchQueryValidator : AbstractValidator<SearchQuery>
{
    public SearchQueryValidator(IAuthorizationService authorizationService)
    {
        RuleFor(q => q.PageIndex)
            .GreaterThanOrEqualTo(1);

        When(_ => authorizationService.IsAdmin() || authorizationService.IsSuperUser(), () =>
        {
            RuleFor(q => q.PageSize)
                .GreaterThanOrEqualTo(1)
                .LessThanOrEqualTo(5000);
        });

        When(_ => authorizationService.IsWriter(), () =>
        {
            RuleFor(q => q.PageSize)
                .GreaterThanOrEqualTo(1)
                .LessThanOrEqualTo(250);
        });

        When(_ => !(authorizationService.IsAdmin() || authorizationService.IsSuperUser() || authorizationService.IsWriter()), () =>
        {
            RuleFor(q => q.PageSize)
               .GreaterThanOrEqualTo(1)
               .LessThanOrEqualTo(100);
        });

        When(_ => authorizationService.IsSuperUser(), () =>
        {
            RuleFor(q => q.ModifiedFrom)
                .Empty();
            RuleFor(q => q.ModifiedTo)
                .Empty();
            RuleFor(q => q.LenochClassifications)
                .Empty();
            RuleFor(q => q.ModifiedPeriodPriority)
                .Equal(false);
            RuleFor(q => q.LenochClassificationsPriority)
                .Equal(false);
        });

        When(_ => authorizationService.IsWriter(), () =>
        {
            RuleFor(q => q.CreatedBy)
                .Empty();
            RuleFor(q => q.ModifiedBy)
                .Empty();
            RuleFor(q => q.ModifiedFrom)
                .Empty();
            RuleFor(q => q.ModifiedTo)
                .Empty();
            RuleFor(q => q.LenochClassifications)
                .Empty();
            RuleFor(q => q.ModifiedPeriodPriority)
                .Equal(false);
            RuleFor(q => q.LenochClassificationsPriority)
                .Equal(false);
        });
    }
}
