using Bk.Termdat.Business.Resource.Models;
using FluentValidation;

namespace Bk.Termdat.Business.Resource.Validators;

public class LanguageUpdateValidator : LanguageModificationValidator<LanguageUpdate>
{
    public LanguageUpdateValidator()
    {
        RuleFor(e => e.Id).NotEmpty();
    }
}
