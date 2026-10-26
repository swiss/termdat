using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bk.Termdat.Business.Domain.Models;
using Bk.Termdat.Business.Repositories;
using Bk.Termdat.Business.Resource.Models;
using Bk.Termdat.CrossCutting;

namespace Bk.Termdat.Business.Mapping;

public class LanguageNewsTranslationMapper : ILanguageNewsTranslationMapper
{
    private readonly ILanguageRepository languageRepository;

    public LanguageNewsTranslationMapper(ILanguageRepository languageRepository)
    {
        this.languageRepository = languageRepository;
    }

    public async Task<IEnumerable<NewsItemTranslation>> Map(ILanguageNewsTranslation source)
    {
        var languages = (await languageRepository.GetAll()).ToList();

        var usedLanguages = languages.Where(l =>
            IsoCode.De.IsEqualTo(l.IsoCode) ||
            IsoCode.Fr.IsEqualTo(l.IsoCode) ||
            IsoCode.It.IsEqualTo(l.IsoCode) ||
            IsoCode.Rm.IsEqualTo(l.IsoCode) ||
            IsoCode.En.IsEqualTo(l.IsoCode));

        return usedLanguages
            .Select(l => MapTranslation(source, l))
            .Where(t => t != null);
    }

    public void MapTranslationsTo(IEnumerable<NewsItemTranslation> translations, ILanguageNewsTranslation destination)
    {
        ArgumentNullException.ThrowIfNull(translations);
        ArgumentNullException.ThrowIfNull(destination);

        foreach (var translation in translations)
        {
            switch (translation.Language.IsoCode)
            {
                case var c when IsoCode.De.IsEqualTo(c):
                    destination.TitleDe = translation.Title;
                    destination.TextDe = translation.Text;
                    break;

                case var c when IsoCode.Fr.IsEqualTo(c):
                    destination.TitleFr = translation.Title;
                    destination.TextFr = translation.Text;
                    break;

                case var c when IsoCode.It.IsEqualTo(c):
                    destination.TitleIt = translation.Title;
                    destination.TextIt = translation.Text;
                    break;

                case var c when IsoCode.Rm.IsEqualTo(c):
                    destination.TitleRm = translation.Title;
                    destination.TextRm = translation.Text;
                    break;

                case var c when IsoCode.En.IsEqualTo(c):
                    destination.TitleEn = translation.Title;
                    destination.TextEn = translation.Text;
                    break;

                default:
                    break;
            }
        }
    }

    private static NewsItemTranslation? MapTranslation(ILanguageNewsTranslation source, Language language)
    {
        var (title, text) = language.IsoCode switch
        {
            var c when IsoCode.De.IsEqualTo(c) => (source.TitleDe, source.TextDe),
            var c when IsoCode.Fr.IsEqualTo(c) => (source.TitleFr, source.TextFr),
            var c when IsoCode.It.IsEqualTo(c) => (source.TitleIt, source.TextIt),
            var c when IsoCode.Rm.IsEqualTo(c) => (source.TitleRm, source.TextRm),
            var c when IsoCode.En.IsEqualTo(c) => (source.TitleEn, source.TextEn),
            _ => (null, null)
        };

        if (title is null && text is null)
        {
            return null;
        }

        return new NewsItemTranslation
        {
            Language = language,
            LanguageId = language.Id,
            Title = title,
            Text = text
        };
    }
}
