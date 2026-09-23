using Bk.Termdat.Business.Resource.Models;
using FluentValidation;

namespace Bk.Termdat.Business.Resource.Validators;

public class OfficeUpdateValidator : OfficeModificationValidator<OfficeUpdate>
{
    public OfficeUpdateValidator()
    {
        RuleFor(e => e.Id).NotEmpty();

    }
}
