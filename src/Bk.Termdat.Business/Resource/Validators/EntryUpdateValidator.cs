using Bk.Termdat.Business.Resource.Models;
using FluentValidation;

namespace Bk.Termdat.Business.Resource.Validators;

public class EntryUpdateValidator : EntryModificationValidator<EntryUpdate>
{
    public EntryUpdateValidator()
    {
        RuleFor(e => e.Id)
            .NotEmpty();

        RuleFor(e => e.Picture)
            .SetValidator(new PictureValidator());
    }
}
