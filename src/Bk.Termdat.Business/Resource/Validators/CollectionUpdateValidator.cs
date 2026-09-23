using Bk.Termdat.Business.Resource.Models;
using FluentValidation;

namespace Bk.Termdat.Business.Resource.Validators;

public class CollectionUpdateValidator : CollectionModificationValidator<CollectionUpdate>
{
    public CollectionUpdateValidator()
    {
        RuleFor(e => e.Id).NotEmpty();
    }
}
