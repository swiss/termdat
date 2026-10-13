using Bk.Termdat.LindasExport.Api.Extensions;
using Bk.Termdat.LindasExport.Api.Models;

namespace Bk.Termdat.LindasExport.Api.Tests.Extensions;

[TestFixture]
internal class LanguageExtensionTests
{
    [Test]
    public void ToLowerCaseStringValue_ForUnknownLanguage_ShouldThrowArgumentOutOfRangeException()
    {
        const Language language = (Language)40;

        Assert.That(() => language.ToLowerCaseStringValue(), Throws.InstanceOf<ArgumentOutOfRangeException>());
    }

    [TestCase(Language.De, "de")]
    [TestCase(Language.Fr, "fr")]
    [TestCase(Language.It, "it")]
    [TestCase(Language.Rm, "rm")]
    [TestCase(Language.En, "en")]
    public void ToLowerCaseStringValue_ForLanguages_ShouldMap(Language language, string expected)
    {
        var result = language.ToLowerCaseStringValue();

        Assert.That(result, Is.EqualTo(expected));
    }

    [TestCase("DE", Language.De)]
    [TestCase("FR", Language.Fr)]
    [TestCase("IT", Language.It)]
    [TestCase("RM", Language.Rm)]
    [TestCase("EN", Language.En)]
    public void ToSparqlLanguage_WhenCalled_ReturnsLanguage(string language, Language expectedLanguage)
    {
        var languageModel = new Business.Domain.Models.Language { IsoCode = language };

        Assert.That(languageModel.ToSparqlLanguage(), Is.EqualTo(expectedLanguage));
    }

    [TestCase("FI")]
    [TestCase("Dk")]
    [TestCase("")]
    public void ToSparqlLanguage_WhenCalledWithUnsupportedLanguage_ThrowsArgumentOutOfRangeException(string language)
    {
        var languageModel = new Business.Domain.Models.Language { IsoCode = language };

        Assert.That(() => languageModel.ToSparqlLanguage(), Throws.Exception.InstanceOf<ArgumentOutOfRangeException>());
    }

    [Test]
    public void FilterSupportedLanguages_WhenCalled_KeepsOnlySupportedLanguages()
    {
        var translations = new[] { "DE", "FR", "IT", "RM", "EN", "XY" }
            .Select(isoCode => new Business.Domain.Models.OfficeTranslation
            {
                Language = new Business.Domain.Models.Language { IsoCode = isoCode }
            });

        var result = translations.FilterSupportedLanguages().Select(translation => translation.Language.IsoCode);

        Assert.That(result, Is.EqualTo(new[] { "DE", "FR", "IT", "RM", "EN" }));
    }
}
