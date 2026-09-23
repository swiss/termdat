using System.Collections.Generic;
using Bk.Termdat.Business.Domain.Models;

namespace Bk.Termdat.Business.Repositories;

public interface INoteSubCodeRepository
{
    IReadOnlyCollection<NoteSubCodeTranslation> GetTranslations();
}
