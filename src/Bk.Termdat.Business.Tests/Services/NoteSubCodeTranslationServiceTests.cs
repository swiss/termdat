using Bk.Termdat.Business.Domain.Models;
using Bk.Termdat.Business.Repositories;
using Bk.Termdat.Business.Services;
using Bk.Termdat.Business.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using NSubstitute;
using NSubstitute.ClearExtensions;
using NUnit.Framework;

namespace Bk.Termdat.Business.Tests.Services;

[TestFixture]
internal class NoteSubCodeTranslationServiceTests
{
    private NoteSubCodeTranslationService service;
    private INoteSubCodeRepository noteSubCodeRepository;
    private ICultureService cultureService;

    [SetUp]
    public void SetUp()
    {
        noteSubCodeRepository = Substitute.For<INoteSubCodeRepository>();
        cultureService = Substitute.For<ICultureService>();
        cultureService.GetCurrentIsoCode().Returns("DE");
        noteSubCodeRepository.GetTranslations().Returns([
            new NoteSubCodeTranslation
            {
                Name = "German tooltip",
                LanguageId = 1,
                Language = new Language { Id = 1, IsoCode = "de" },
                NoteSubCode = new NoteSubCode { Code = "ABC" }
            },
            new NoteSubCodeTranslation
            {
                Name = "English tooltip",
                LanguageId = 3,
                Language = new Language { Id = 3, IsoCode = "en" },
                NoteSubCode = new NoteSubCode { Code = "ABC" }
            }
        ]);

        service = new NoteSubCodeTranslationService(
            noteSubCodeRepository,
            Substitute.For<IMemoryCache>(),
            cultureService);
    }

    [TearDown]
    public void TearDown()
    {
        noteSubCodeRepository.ClearSubstitute();
    }

    [Test]
    public void GetTooltips_WhenNoteIsNullOrWhitespace_ShouldReturnEmptyList()
    {
        var result = service.GetTooltips(null);

        Assert.That(result, Is.Empty);
    }

    [Test]
    public void GetTooltips_WhenTranslationExists_ShouldReturnTranslatedTooltip()
    {
        noteSubCodeRepository
            .GetTranslations()
            .Returns([
                new NoteSubCodeTranslation
                {
                    Name = "German tooltip",
                    LanguageId = 1,
                    Language = new Language { Id = 1, IsoCode = "DE" },
                    NoteSubCode = new NoteSubCode
                    {
                        Code = "ABC",
                    }
                }]);

        cultureService.GetCurrentIsoCode().Returns("DE");

        var result = service.GetTooltips("ABC: apple");

        Assert.That(result, Has.Count.EqualTo(1));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result[0].Code, Is.EqualTo("ABC"));
            Assert.That(result[0].Tooltip, Is.EqualTo("German tooltip"));
        }
    }

    [Test]
    public void GetTooltips_WhenLanguageMissing_ShouldFallbackToEnglish()
    {
        noteSubCodeRepository
            .GetTranslations()
            .Returns([
                new NoteSubCodeTranslation
                {
                    Name = "English tooltip",
                    LanguageId = 3,
                    Language = new Language { Id = 3, IsoCode = "EN" },
                    NoteSubCode = new NoteSubCode
                    {
                        Code = "ABC",
                    }
                }]);

        cultureService.GetCurrentIsoCode().Returns("FR");

        var result = service.GetTooltips("ABC: apple");

        Assert.That(result[0].Tooltip, Is.EqualTo("English tooltip"));
    }

    [Test]
    public void GetTooltips_WhenUiCultureUsesRegionalGerman_ShouldResolveGermanTranslation()
    {
        noteSubCodeRepository
            .GetTranslations()
            .Returns([
                new NoteSubCodeTranslation
                {
                    Name = "German tooltip",
                    LanguageId = 1,
                    Language = new Language { Id = 1, IsoCode = "DE" },
                    NoteSubCode = new NoteSubCode
                    {
                        Code = "ABC",
                    }
                },
                new NoteSubCodeTranslation
                {
                    Name = "English tooltip",
                    LanguageId = 3,
                    Language = new Language { Id = 3, IsoCode = "EN" },
                    NoteSubCode = new NoteSubCode
                    {
                        Code = "ABC",
                    }
                }]);

        cultureService.GetCurrentIsoCode().Returns("de-CH");

        var result = service.GetTooltips("ABC: apple");

        Assert.That(result[0].Tooltip, Is.EqualTo("German tooltip"));
    }

    [Test]
    public void GetTooltips_WhenLanguageIdIsZero_ShouldStillUseEnglishFallback()
    {
        noteSubCodeRepository
            .GetTranslations()
            .Returns([
                new NoteSubCodeTranslation
                {
                    Name = "English tooltip",
                    LanguageId = 3,
                    Language = new Language { Id = 3, IsoCode = "EN" },
                    NoteSubCode = new NoteSubCode
                    {
                        Code = "ABC",
                    }
                }]);

        cultureService.GetCurrentIsoCode().Returns("ZZ");

        var result = service.GetTooltips("ABC: apple");

        Assert.That(result[0].Tooltip, Is.EqualTo("English tooltip"));
    }

    [Test]
    public void GetTooltips_WhenLanguageIdLookupMisses_ShouldNotThrowAndUseEnglishFallback()
    {
        noteSubCodeRepository
            .GetTranslations()
            .Returns([
                new NoteSubCodeTranslation
                {
                    Name = "English tooltip",
                    LanguageId = 3,
                    Language = new Language { Id = 3, IsoCode = "EN" },
                    NoteSubCode = new NoteSubCode
                    {
                        Code = "ABC",
                    }
                }]);

        cultureService.GetCurrentIsoCode().Returns("de-CH");

        var result = service.GetTooltips("ABC: apple");

        Assert.That(result[0].Tooltip, Is.EqualTo("English tooltip"));
    }

    [Test]
    public void GetTooltips_ShouldReturnMultipleCodes()
    {
        noteSubCodeRepository
            .GetTranslations()
            .Returns([
                new NoteSubCodeTranslation
                {
                    Name = "Deutsch Apfel tooltip",
                    LanguageId = 1,
                    Language = new Language { Id = 1, IsoCode = "DE" },
                    NoteSubCode = new NoteSubCode
                    {
                        Code = "ABC",
                    }
                },
                new NoteSubCodeTranslation
                {
                    Name = "Deutsch Birne tooltip",
                    LanguageId = 1,
                    Language = new Language { Id = 1, IsoCode = "DE" },
                    NoteSubCode = new NoteSubCode
                    {
                        Code = "DEF",
                    }
                }]);

        cultureService.GetCurrentIsoCode().Returns("DE");

        var result = service.GetTooltips("ABC: apple; DEF: pear");

        Assert.That(result, Has.Count.EqualTo(2));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result[0].Code, Is.EqualTo("ABC"));
            Assert.That(result[1].Code, Is.EqualTo("DEF"));
            Assert.That(result[0].Tooltip, Is.EqualTo("Deutsch Apfel tooltip"));
            Assert.That(result[1].Tooltip, Is.EqualTo("Deutsch Birne tooltip"));
        }
    }

    [Test]
    public void GetTooltips_ShouldIgnoreInvalidPatterns()
    {
        noteSubCodeRepository
            .GetTranslations()
            .Returns([
                new NoteSubCodeTranslation
                {
                    Name = "English tooltip",
                    LanguageId = 1,
                    Language = new Language { Id = 1, IsoCode = "DE" },
                    NoteSubCode = new NoteSubCode
                    {
                        Code = "ABC",
                    }
                }]);

        var result = service.GetTooltips("ABC: text KORTE: value");

        Assert.That(result, Has.Count.EqualTo(1));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result[0].Code, Is.EqualTo("ABC"));
            Assert.That(result[0].Tooltip, Is.EqualTo("English tooltip"));
        }
    }

    [Test]
    public void GetTooltips_ShouldNormalizeCodeToUppercase()
    {
        noteSubCodeRepository
           .GetTranslations()
           .Returns([
                new NoteSubCodeTranslation
                {
                    Name = "Deutsch tooltip",
                    LanguageId = 1,
                    Language = new Language { Id = 1, IsoCode = "DE" },
                    NoteSubCode = new NoteSubCode
                    {
                        Code = "ABC",
                    }
                }]);

        var result = service.GetTooltips(" abc : apple ");

        Assert.That(result, Has.Count.EqualTo(1));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result[0].Code, Is.EqualTo("ABC"));
            Assert.That(result[0].Tooltip, Is.EqualTo("Deutsch tooltip"));
        }
    }

    [Test]
    public void GetTooltips_ShouldReturnNullTooltip()
    {
        noteSubCodeRepository
            .GetTranslations()
            .Returns([]);

        var result = service.GetTooltips("ABC: apple");

        Assert.That(result[0].Tooltip, Is.Null);
    }
}
