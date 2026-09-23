using Bk.Termdat.Business.Resource.Models;
using FluentValidation;

namespace Bk.Termdat.Business.Resource.Validators;

public abstract class ConceptSystemModificationValidator<T, TPosition> : AbstractValidator<T> where T : IConceptSystemModification<TPosition>
    where TPosition : IConceptSystemPositionModification
{
    protected ConceptSystemModificationValidator()
    {
        RuleFor(p => p.CollectionId).NotEmpty();
        RuleFor(p => p.Title).NotNull().NotEmpty().MaximumLength(1000);
        RuleForEach(p => p.Positions).ChildRules(positions =>
        {
            positions.RuleFor(p => p.Value).MaximumLength(1000);
        });
    }
}
