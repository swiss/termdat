using System.Collections.Generic;
using System.Linq;
using Bk.Termdat.Business.Mapping;
using NUnit.Framework;
using DomainModels = Bk.Termdat.Business.Domain.Models;
using ResourceModels = Bk.Termdat.Business.Resource.Models;

namespace Bk.Termdat.Business.Tests.Mapping;

[TestFixture]
internal class PictureTranslationMapperTests
{
    private ResourceModels.PictureDetail dto;
    private List<DomainModels.Language> languages;
    private readonly PictureTranslationMapper mapper = new();

    [SetUp]
    public void SetUp()
    {
        dto = new()
        {
            EntryId = 10,
            DescriptionDe = "DescriptionDe",
            DescriptionEn = "DescriptionEn",
            DescriptionFr = "DescriptionFr",
            DescriptionIt = "DescriptionIt",
            DescriptionRm = "DescriptionRm",
            SourceDe = "SourceDe",
            SourceEn = "SourceEn",
            SourceFr = "SourceFr",
            SourceIt = "SourceIt",
            SourceRm = "SourceRm",
        };
        languages = new()
        {
            new() { Id = 1, IsoCode = "DE" },
            new() { Id = 2, IsoCode = "EN" },
            new() { Id = 3, IsoCode = "FR" },
            new() { Id = 4, IsoCode = "IT" },
            new() { Id = 5, IsoCode = "RM" },
            new() { Id = 6, IsoCode = "ES" },
        };
    }

    [Test]
    public void CanMapFromLanguage()
    {
        var translationDe = mapper.Map(dto, languages[0]);
        Assert.That(translationDe, Is.Not.Null);
        Assert.That(translationDe.EntryId, Is.EqualTo(dto.EntryId));
        Assert.That(translationDe.LanguageId, Is.EqualTo(languages[0].Id));
        Assert.That(translationDe.Description, Is.EqualTo(dto.DescriptionDe));
        Assert.That(translationDe.Source, Is.EqualTo(dto.SourceDe));

        var translationFr = mapper.Map(dto, languages[2]);
        Assert.That(translationFr, Is.Not.Null);
        Assert.That(translationFr.EntryId, Is.EqualTo(dto.EntryId));
        Assert.That(translationFr.LanguageId, Is.EqualTo(languages[2].Id));
        Assert.That(translationFr.Description, Is.EqualTo(dto.DescriptionFr));
        Assert.That(translationFr.Source, Is.EqualTo(dto.SourceFr));

        var translationIt = mapper.Map(dto, languages[3]);
        Assert.That(translationIt, Is.Not.Null);
        Assert.That(translationIt.EntryId, Is.EqualTo(dto.EntryId));
        Assert.That(translationIt.LanguageId, Is.EqualTo(languages[3].Id));
        Assert.That(translationIt.Description, Is.EqualTo(dto.DescriptionIt));
        Assert.That(translationIt.Source, Is.EqualTo(dto.SourceIt));

        var translationRm = mapper.Map(dto, languages[4]);
        Assert.That(translationRm, Is.Not.Null);
        Assert.That(translationRm.EntryId, Is.EqualTo(dto.EntryId));
        Assert.That(translationRm.LanguageId, Is.EqualTo(languages[4].Id));
        Assert.That(translationRm.Description, Is.EqualTo(dto.DescriptionRm));
        Assert.That(translationRm.Source, Is.EqualTo(dto.SourceRm));

        var translationEn = mapper.Map(dto, languages[1]);
        Assert.That(translationEn, Is.Not.Null);
        Assert.That(translationEn.EntryId, Is.EqualTo(dto.EntryId));
        Assert.That(translationEn.LanguageId, Is.EqualTo(languages[1].Id));
        Assert.That(translationEn.Description, Is.EqualTo(dto.DescriptionEn));
        Assert.That(translationEn.Source, Is.EqualTo(dto.SourceEn));
    }

    [Test]
    public void CanMapFromLanguageReturnsNullForUnmappedLanguages()
    {
        var translationNull = mapper.Map(dto, languages[5]);
        Assert.That(translationNull, Is.Null);

        dto.DescriptionDe = null;
        dto.SourceDe = null;

        var translationEmpty = mapper.Map(dto, languages[0]);
        Assert.That(translationEmpty, Is.Null);
    }

    [Test]
    public void CanMapFromLanguages()
    {
        var translations = mapper.Map(dto, languages).ToList();
        Assert.That(translations, Is.Not.Null);
        Assert.That(translations.Count, Is.EqualTo(5));

        var translationDe = translations.First(t => t.LanguageId == 1);
        Assert.That(translationDe, Is.Not.Null);
        Assert.That(translationDe.EntryId, Is.EqualTo(dto.EntryId));
        Assert.That(translationDe.LanguageId, Is.EqualTo(languages[0].Id));
        Assert.That(translationDe.Description, Is.EqualTo(dto.DescriptionDe));
        Assert.That(translationDe.Source, Is.EqualTo(dto.SourceDe));

        var translationFr = translations.First(t => t.LanguageId == 3);
        Assert.That(translationFr, Is.Not.Null);
        Assert.That(translationFr.EntryId, Is.EqualTo(dto.EntryId));
        Assert.That(translationFr.LanguageId, Is.EqualTo(languages[2].Id));
        Assert.That(translationFr.Description, Is.EqualTo(dto.DescriptionFr));
        Assert.That(translationFr.Source, Is.EqualTo(dto.SourceFr));

        var translationIt = translations.First(t => t.LanguageId == 4);
        Assert.That(translationIt, Is.Not.Null);
        Assert.That(translationIt.EntryId, Is.EqualTo(dto.EntryId));
        Assert.That(translationIt.LanguageId, Is.EqualTo(languages[3].Id));
        Assert.That(translationIt.Description, Is.EqualTo(dto.DescriptionIt));
        Assert.That(translationIt.Source, Is.EqualTo(dto.SourceIt));

        var translationRm = translations.First(t => t.LanguageId == 5);
        Assert.That(translationRm, Is.Not.Null);
        Assert.That(translationRm.EntryId, Is.EqualTo(dto.EntryId));
        Assert.That(translationRm.LanguageId, Is.EqualTo(languages[4].Id));
        Assert.That(translationRm.Description, Is.EqualTo(dto.DescriptionRm));
        Assert.That(translationRm.Source, Is.EqualTo(dto.SourceRm));

        var translationEn = translations.First(t => t.LanguageId == 2);
        Assert.That(translationEn, Is.Not.Null);
        Assert.That(translationEn.EntryId, Is.EqualTo(dto.EntryId));
        Assert.That(translationEn.LanguageId, Is.EqualTo(languages[1].Id));
        Assert.That(translationEn.Description, Is.EqualTo(dto.DescriptionEn));
        Assert.That(translationEn.Source, Is.EqualTo(dto.SourceEn));
    }

    [Test]
    public void CanMapFromLanguagesDoesNotReturnEmptyTranslations()
    {
        dto.DescriptionDe = null;
        dto.SourceDe = null;

        var translations = mapper.Map(dto, languages).ToList();
        Assert.That(translations, Is.Not.Null);
        Assert.That(translations.Count, Is.EqualTo(4));
        Assert.That(translations.TrueForAll(t => t.LanguageId != 1));
    }
}
