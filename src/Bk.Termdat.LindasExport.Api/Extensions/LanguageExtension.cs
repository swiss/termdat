using Bk.Termdat.Business.Domain.Models;
using Language = Bk.Termdat.LindasExport.Api.Models.Language;

namespace Bk.Termdat.LindasExport.Api.Extensions;

public static class LanguageExtension
{
    public static string ToLowerCaseStringValue(this Language language)
    {
        return language switch
        {
            Language.De => "de",
            Language.Fr => "fr",
            Language.It => "it",
            Language.Rm => "rm",
            Language.En => "en",
            _ => throw new ArgumentOutOfRangeException(nameof(language), language, null)
        };
    }

    public static Language ToSparqlLanguage(this Business.Domain.Models.Language language)
    {
        return language.IsoCode switch
        {
            "DE" => Language.De,
            "FR" => Language.Fr,
            "IT" => Language.It,
            "RM" => Language.Rm,
            "EN" => Language.En,
            _ => throw new ArgumentOutOfRangeException(nameof(language), language,
                $"Language={language.IsoCode} not supported")
        };
    }

    public static IEnumerable<T> FilterSupportedLanguages<T>(this IEnumerable<T> translations)
        where T : IEntityTranslation
    {
        return translations.Where(item => item.Language.IsoCode is "DE" or "FR" or "IT" or "RM" or "EN");
    }
}
