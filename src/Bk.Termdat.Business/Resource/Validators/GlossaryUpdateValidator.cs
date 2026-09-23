using Bk.Termdat.Business.Resource.Models;
using FluentValidation;

namespace Bk.Termdat.Business.Resource.Validators;

public class GlossaryUpdateValidator : GlossaryModificationValidator<GlossaryUpdate>
{
    public GlossaryUpdateValidator()
    {
        RuleFor(e => e.Id).NotEmpty();
    }
}
