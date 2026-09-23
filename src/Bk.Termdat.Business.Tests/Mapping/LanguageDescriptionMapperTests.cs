using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bk.Termdat.Business.Mapping;
using Bk.Termdat.Business.Repositories;
using NSubstitute;
using NUnit.Framework;
using DomainModels = Bk.Termdat.Business.Domain.Models;
using ResourceModels = Bk.Termdat.Business.Resource.Models;

namespace Bk.Termdat.Business.Tests.Mapping;

internal class TestModification : ResourceModels.ILanguageDescription
{
    public string DescriptionEn { get; set; }
    public string DescriptionDe { get; set; }
    public string DescriptionFr { get; set; }
    public string DescriptionIt { get; set; }
    public string DescriptionRm { get; set; }
}

internal class TestTranslation : DomainModels.IEntityNameTranslation
{
    public int LanguageId { get; set; }
    public DomainModels.Language Language { get; set; }
    public string Name { get; set; }
}

[TestFixture]
public class LanguageDescriptionMapperTests
{
    private ILanguageDescriptionMapper mapper;
    private ILanguageRepository languageRepositoryMock;
    private ResourceModels.ILanguageDescription testModification;
    private List<DomainModels.Language> languages;

    [SetUp]
    public void Setup()
    {

        testModification = new TestModification
        {
            DescriptionDe = "de",
            DescriptionFr = "fr",
            DescriptionEn = "en",
            DescriptionIt = "it",
            DescriptionRm = "rm"
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

        languageRepositoryMock = Substitute.For<ILanguageRepository>();
        languageRepositoryMock.GetAll().Returns(languages);
        mapper = new LanguageDescriptionMapper(languageRepositoryMock);
    }

    [Test]
    public async Task MapFromLanguagesShouldMapLanguages()
    {
        var translations = (await mapper.Map<TestTranslation>(testModification)).ToList();
        Assert.That(translations, Is.Not.Null);
        Assert.That(translations.Count, Is.EqualTo(5));

        var translationDe = translations.First(t => t.LanguageId == 1);
        AssertTranslationIsCorrect(translationDe, 1, testModification.DescriptionDe);

        var translationEn = translations.First(t => t.LanguageId == 2);
        AssertTranslationIsCorrect(translationEn, 2, testModification.DescriptionEn);

        var translationFr = translations.First(t => t.LanguageId == 3);
        AssertTranslationIsCorrect(translationFr, 3, testModification.DescriptionFr);

        var translationIt = translations.First(t => t.LanguageId == 4);
        AssertTranslationIsCorrect(translationIt, 4, testModification.DescriptionIt);

        var translationRm = translations.First(t => t.LanguageId == 5);
        AssertTranslationIsCorrect(translationRm, 5, testModification.DescriptionRm);
    }

    [Test]
    public async Task MapFromLanguagesShouldNotReturnEmptyTranslations()
    {
        testModification.DescriptionDe = null;

        var translations = (await mapper.Map<TestTranslation>(testModification)).ToList();
        Assert.That(translations, Is.Not.Null);
        Assert.That(translations.Count, Is.EqualTo(4));
        Assert.That(translations.TrueForAll(t => t.LanguageId != 1));
    }

    private static void AssertTranslationIsCorrect(DomainModels.IEntityNameTranslation translation, int languageId, string description)
    {
        Assert.That(translation, Is.Not.Null);
        Assert.That(translation.LanguageId, Is.EqualTo(languageId));
        Assert.That(translation.Name, Is.EqualTo(description));
    }
}
