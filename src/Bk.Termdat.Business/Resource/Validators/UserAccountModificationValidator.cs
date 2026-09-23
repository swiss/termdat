using Bk.Termdat.Business.Resource.Models;
using Bk.Termdat.Common.Resources;
using FluentValidation;

namespace Bk.Termdat.Business.Resource.Validators;

public class UserAccountModificationValidator : AbstractValidator<UserAccountModification>
{
    public UserAccountModificationValidator()
    {
        RuleFor(u => u.Information)
            .MaximumLength(1000);

        RuleFor(u => u.AuthorizedCollectionIds)
            .NotNull()
            .NotEmpty()
            .WithName(BackendTexts.EntryCollection);
    }
}
