using System;
using System.Collections.Generic;
using System.Linq;
using Bk.Termdat.Business.Domain.Models;
using Bk.Termdat.CrossCutting;
using ResourceModels = Bk.Termdat.Business.Resource.Models;

namespace Bk.Termdat.Business.Mapping;

public class PictureTranslationMapper : IPictureTranslationMapper
{
    public IEnumerable<PictureTranslation> Map(ResourceModels.PictureDetail pictureDto, IEnumerable<Language> languages)
    {
        var usedLanguages = languages.Where(l => IsoCode.De.IsEqualTo(l.IsoCode) || IsoCode.Fr.IsEqualTo(l.IsoCode)
            || IsoCode.It.IsEqualTo(l.IsoCode) || IsoCode.Rm.IsEqualTo(l.IsoCode) || IsoCode.En.IsEqualTo(l.IsoCode));

        return usedLanguages
            .Select(l => Map(pictureDto, l))
            .Where(l => l != null);
    }

    public PictureTranslation Map(ResourceModels.PictureDetail pictureDto, Language language)
    {
        ArgumentNullException.ThrowIfNull(pictureDto);
        ArgumentNullException.ThrowIfNull(language);

        var translation = new PictureTranslation
        {
            LanguageId = language.Id,
            EntryId = pictureDto.EntryId ?? 0,
        };

        if (IsoCode.De.IsEqualTo(language.IsoCode))
        {
            translation.Description = pictureDto.DescriptionDe;
            translation.Source = pictureDto.SourceDe;
        }
        else if (IsoCode.Fr.IsEqualTo(language.IsoCode))
        {
            translation.Description = pictureDto.DescriptionFr;
            translation.Source = pictureDto.SourceFr;
        }
        else if (IsoCode.It.IsEqualTo(language.IsoCode))
        {
            translation.Description = pictureDto.DescriptionIt;
            translation.Source = pictureDto.SourceIt;
        }
        else if (IsoCode.Rm.IsEqualTo(language.IsoCode))
        {
            translation.Description = pictureDto.DescriptionRm;
            translation.Source = pictureDto.SourceRm;
        }
        else if (IsoCode.En.IsEqualTo(language.IsoCode))
        {
            translation.Description = pictureDto.DescriptionEn;
            translation.Source = pictureDto.SourceEn;
        }
        else
        {
            return null;
        }

        if (translation.Description == null && translation.Source == null)
        {
            return null;
        }

        return translation;
    }
}
