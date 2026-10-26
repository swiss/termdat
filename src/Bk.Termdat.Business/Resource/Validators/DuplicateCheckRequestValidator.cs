using Bk.Termdat.Business.Resource.Models;
using FluentValidation;

namespace Bk.Termdat.Business.Resource.Validators;

public class DuplicateCheckRequestValidator
    : AbstractValidator<DuplicateCheckRequest>
{
    public DuplicateCheckRequestValidator()
    {
        RuleFor(x => x).Must(x => x.CollectionId.HasValue ^ x.GlossaryId.HasValue);
    }
}
