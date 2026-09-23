using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bk.Termdat.Business.Domain.Models;
using Bk.Termdat.Business.Repositories;
using Bk.Termdat.Business.Resource.Models;
using Bk.Termdat.CrossCutting;

namespace Bk.Termdat.Business.Mapping;

public class LanguageDescriptionMapper : ILanguageDescriptionMapper
{
    private readonly ILanguageRepository languageRepository;

    public LanguageDescriptionMapper(ILanguageRepository languageRepository)
    {
        this.languageRepository = languageRepository;
    }

    public static void MapTranslationsToLanguageDescription<T>(ILanguageDescription dest, ICollection<T> translations)
        where T : IEntityNameTranslation
    {
        dest.DescriptionDe = translations.FirstOrDefault(t => IsoCode.De.IsEqualTo(t.Language.IsoCode))?.Name;
        dest.DescriptionFr = translations.FirstOrDefault(t => IsoCode.Fr.IsEqualTo(t.Language.IsoCode))?.Name;
        dest.DescriptionIt = translations.FirstOrDefault(t => IsoCode.It.IsEqualTo(t.Language.IsoCode))?.Name;
        dest.DescriptionRm = translations.FirstOrDefault(t => IsoCode.Rm.IsEqualTo(t.Language.IsoCode))?.Name;
        dest.DescriptionEn = translations.FirstOrDefault(t => IsoCode.En.IsEqualTo(t.Language.IsoCode))?.Name;
    }

    public async Task<IEnumerable<T>> Map<T>(ILanguageDescription languageDescription) where T : IEntityNameTranslation, new()
    {
        var languages = (await languageRepository.GetAll()).ToList();

        var usedLanguages = languages.Where(l => IsoCode.De.IsEqualTo(l.IsoCode) || IsoCode.Fr.IsEqualTo(l.IsoCode)
            || IsoCode.It.IsEqualTo(l.IsoCode) || IsoCode.Rm.IsEqualTo(l.IsoCode) || IsoCode.En.IsEqualTo(l.IsoCode));

        return usedLanguages.Select(l => MapLanguageDescriptionToTranslation<T>(languageDescription, l)).Where(l => l != null);
    }

    private static T MapLanguageDescriptionToTranslation<T>(ILanguageDescription languageDescription, Language language)
        where T : IEntityNameTranslation, new()
    {
        var name = language.IsoCode switch
        {
            var c when IsoCode.De.IsEqualTo(c) => languageDescription.DescriptionDe,
            var c when IsoCode.Fr.IsEqualTo(c) => languageDescription.DescriptionFr,
            var c when IsoCode.It.IsEqualTo(c) => languageDescription.DescriptionIt,
            var c when IsoCode.Rm.IsEqualTo(c) => languageDescription.DescriptionRm,
            var c when IsoCode.En.IsEqualTo(c) => languageDescription.DescriptionEn,
            _ => null
        };

        return name is null ? default : new T
        {
            LanguageId = language.Id,
            Name = name
        };
    }
}
