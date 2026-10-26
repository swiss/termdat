using System;
using System.Collections.Generic;
using System.Linq;
using Bk.Termdat.Business.Domain.Models;
using Bk.Termdat.Business.Resource.Models;
using Bk.Termdat.CrossCutting;

namespace Bk.Termdat.Business.Mapping;

public class LanguageTranslationMapper : ILanguageTranslationMapper
{
    public IEnumerable<LanguageTranslation> Map(ILanguageModification languageModification, IEnumerable<Language> languages, int languageId)
    {
        var usedLanguages = languages.Where(l => IsoCode.De.IsEqualTo(l.IsoCode) || IsoCode.Fr.IsEqualTo(l.IsoCode)
            || IsoCode.It.IsEqualTo(l.IsoCode) || IsoCode.Rm.IsEqualTo(l.IsoCode) || IsoCode.En.IsEqualTo(l.IsoCode));

        return usedLanguages.Select(l => Map(languageId, languageModification, l)).Where(l => l != null);

    }
    public LanguageTranslation Map(int languageId, ILanguageModification languageModification, Language language)
    {
        ArgumentNullException.ThrowIfNull(language);
        ArgumentNullException.ThrowIfNull(languageModification);

        var translation = new LanguageTranslation
        {
            LanguageId = languageId > 0 ? languageId : language.Id
        };

        if (IsoCode.De.IsEqualTo(language.IsoCode))
        {
            translation.Name = languageModification.DescriptionDe;
            translation.IsoCode = IsoCode.De.ToString().ToUpperInvariant();
        }
        else if (IsoCode.Fr.IsEqualTo(language.IsoCode))
        {
            translation.Name = languageModification.DescriptionFr;
            translation.IsoCode = IsoCode.Fr.ToString().ToUpperInvariant();
        }
        else if (IsoCode.It.IsEqualTo(language.IsoCode))
        {
            translation.Name = languageModification.DescriptionIt;
            translation.IsoCode = IsoCode.It.ToString().ToUpperInvariant();
        }
        else if (IsoCode.Rm.IsEqualTo(language.IsoCode))
        {
            translation.Name = languageModification.DescriptionRm;
            translation.IsoCode = IsoCode.Rm.ToString().ToUpperInvariant();
        }
        else if (IsoCode.En.IsEqualTo(language.IsoCode))
        {
            translation.Name = languageModification.DescriptionEn;
            translation.IsoCode = IsoCode.En.ToString().ToUpperInvariant();
        }
        else
        {
            return default;
        }

        if (translation.Name == null)
        {
            return default;
        }

        return translation;
    }
}
