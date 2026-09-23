using Bk.Termdat.Business.Resource.Models;
using FluentValidation;

namespace Bk.Termdat.Business.Resource.Validators;

public class ConceptSystemUpdateValidator : ConceptSystemModificationValidator<ConceptSystemUpdate, ConceptSystemPositionUpdate>
{
    public ConceptSystemUpdateValidator()
    {
        RuleFor(e => e.Id).NotEmpty();
    }
}
