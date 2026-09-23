using System.Collections.Generic;
using Bk.Termdat.Business.Resource.Models;

namespace Bk.Termdat.Business.Services.Interfaces;

public interface INoteSubCodeTranslationService
{
    IList<NoteSubCodeTooltip> GetTooltips(string note);
}
