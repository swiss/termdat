using System.Collections.Generic;
using Bk.Termdat.Business.Domain.Models;
using Bk.Termdat.Business.Resource.Models;

namespace Bk.Termdat.Business.Mapping;

public interface ILanguageTranslationMapper
{
    IEnumerable<LanguageTranslation> Map(ILanguageModification languageModification, IEnumerable<Language> languages, int languageId);
    LanguageTranslation Map(int languageId, ILanguageModification languageModification, Language language);
}
