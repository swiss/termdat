using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bk.Termdat.Business.Mapping;
using Bk.Termdat.Business.Repositories;
using Bk.Termdat.Business.Resource.Models;
using NSubstitute;
using NSubstitute.ClearExtensions;
using NUnit.Framework;
using DomainModels = Bk.Termdat.Business.Domain.Models;

namespace Bk.Termdat.Business.Tests.Mapping;

[TestFixture]
internal class LanguageNewsTranslationMapperTests
{
    private ILanguageNewsTranslationMapper mapper;
    private ILanguageRepository languageRepositoryMock;
    private List<DomainModels.Language> languages;

    [SetUp]
    public void Setup()
    {
        languages = new()
        {
            new() { Id = 1, IsoCode = "DE" },
            new() { Id = 2, IsoCode = "EN" },
            new() { Id = 3, IsoCode = "FR" },
            new() { Id = 4, IsoCode = "IT" },
            new() { Id = 5, IsoCode = "RM" },
            new() { Id = 6, IsoCode = "ES" },
        };

        languageRepositoryMock = Substitute.For<ILanguageRepository>();
        languageRepositoryMock.GetAll().Returns(languages);
        mapper = new LanguageNewsTranslationMapper(languageRepositoryMock);
    }

    [TearDown]
    public void TearDown()
    {
        languageRepositoryMock.ClearSubstitute();
    }

    [Test]
    public async Task Map_WhenTranslationsExist_ShouldReturnTranslations()
    {
        languageRepositoryMock.GetAll().Returns(new List<DomainModels.Language>
        {
            new() { Id = 1, IsoCode = "de" },
            new() { Id = 2, IsoCode = "fr" },
            new() { Id = 3, IsoCode = "es" }
        });

        var source = new NewsItemCreate
        {
            TitleDe = "Titel",
            TextDe = "Text",
            TitleFr = "Titre",
            TextFr = "Texte",
            TitleIt = "Titolo",
            TextIt = "Texto",
            TitleEn = "Title",
            TextEn = "Text",
            TitleRm = "Titel",
            TextRm = "Text",
        };

        var result = (await mapper.Map(source)).ToList();

        Assert.That(result.Count, Is.EqualTo(2));

        Assert.That(result[0].LanguageId, Is.EqualTo(1));
        Assert.That(result[0].Title, Is.EqualTo("Titel"));
        Assert.That(result[0].Text, Is.EqualTo("Text"));

        Assert.That(result[1].LanguageId, Is.EqualTo(2));
        Assert.That(result[1].Title, Is.EqualTo("Titre"));
        Assert.That(result[1].Text, Is.EqualTo("Texte"));
    }

    [Test]
    public void MapTranslationsTo_WhenCalled_ShouldPopulateDestination()
    {
        var destination = new NewsItemUpdate()
        {
            TitleDe = "Titel",
            TextDe = "Text",
            TitleFr = "Titre",
            TextFr = "Texte",
            TitleIt = "Titolo",
            TextIt = "Texto",
            TitleEn = "Title",
            TextEn = "Text",
            TitleRm = "Titel",
            TextRm = "Text",
        };

        mapper.MapTranslationsTo(
        [
            new()
            {
                Language = new DomainModels.Language { IsoCode = "de" },
                Title = "Titel",
                Text = "Text"
            },
            new()
            {
                Language = new DomainModels.Language { IsoCode = "fr" },
                Title = "Titre",
                Text = "Texte"
            }
        ], destination);

        Assert.That(destination.TitleDe, Is.EqualTo("Titel"));
        Assert.That(destination.TextDe, Is.EqualTo("Text"));

        Assert.That(destination.TitleFr, Is.EqualTo("Titre"));
        Assert.That(destination.TextFr, Is.EqualTo("Texte"));
    }

    [Test]
    public async Task Map_WhenTranslationIsEmpty_ShouldNotReturnTranslation()
    {
        languageRepositoryMock.GetAll().Returns(new List<DomainModels.Language>
        {
            new() { Id = 1, IsoCode = "de" },
            new() { Id = 2, IsoCode = "fr" }
        });

        var source = new NewsItemCreate
        {
            TitleDe = "Titel",
            TextDe = "Text",
            TitleFr = null,
            TextFr = null,
            TitleIt = null,
            TextIt = null,
            TitleEn = null,
            TextEn = null,
            TitleRm = null,
            TextRm = null,
        };

        var result = (await mapper.Map(source)).ToList();

        Assert.That(result.Count, Is.EqualTo(1));

        Assert.That(result[0].LanguageId, Is.EqualTo(1));
        Assert.That(result[0].Title, Is.EqualTo("Titel"));
        Assert.That(result[0].Text, Is.EqualTo("Text"));
    }

    [Test]
    public async Task Map_ShouldIgnoreUnsupportedLanguages()
    {
        languageRepositoryMock.GetAll().Returns(new List<DomainModels.Language>
        {
            new() { Id = 1, IsoCode = "de" },
            new() { Id = 2, IsoCode = "es" },
            new() { Id = 3, IsoCode = "pt" }
        });

        var source = new NewsItemCreate
        {
            TitleDe = "Titel",
            TextDe = "Text",
            TitleFr = "Titre",
            TextFr = "Texte",
            TitleIt = "Titolo",
            TextIt = "Texto",
            TitleEn = "Title",
            TextEn = "Text",
            TitleRm = "Titel",
            TextRm = "Text",
        };

        var result = (await mapper.Map(source)).ToList();

        Assert.That(result.Count, Is.EqualTo(1));

        Assert.That(result[0].LanguageId, Is.EqualTo(1));
        Assert.That(result[0].Title, Is.EqualTo("Titel"));
        Assert.That(result[0].Text, Is.EqualTo("Text"));
    }

    [Test]
    public void MapTranslationsTo_ShouldIgnoreUnsupportedLanguages()
    {
        var destination = new NewsItemUpdate()
        {
            TitleDe = string.Empty,
            TextDe = string.Empty,
            TitleFr = string.Empty,
            TextFr = string.Empty,
            TitleIt = string.Empty,
            TextIt = string.Empty,
            TitleEn = string.Empty,
            TextEn = string.Empty,
            TitleRm = string.Empty,
            TextRm = string.Empty,
        };

        mapper.MapTranslationsTo(
        [
            new DomainModels.NewsItemTranslation
            {
                Language = new DomainModels.Language
                {
                    IsoCode = "es"
                },
                Title = "Titulo",
                Text = "Texto"
            }
        ],
        destination);

        Assert.That(destination.TitleDe, Is.Empty);
        Assert.That(destination.TextDe, Is.Empty);

        Assert.That(destination.TitleFr, Is.Empty);
        Assert.That(destination.TextFr, Is.Empty);

        Assert.That(destination.TitleIt, Is.Empty);
        Assert.That(destination.TextIt, Is.Empty);

        Assert.That(destination.TitleRm, Is.Empty);
        Assert.That(destination.TextRm, Is.Empty);

        Assert.That(destination.TitleEn, Is.Empty);
        Assert.That(destination.TextEn, Is.Empty);
    }
}
