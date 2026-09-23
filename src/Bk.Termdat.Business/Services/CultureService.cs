using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Bk.Termdat.Business.Domain.Models;
using Bk.Termdat.Business.Services.Interfaces;
using Bk.Termdat.CrossCutting;

namespace Bk.Termdat.Business.Services;

public class CultureService : ICultureService
{
    private readonly CultureInfo defaultCulture;

    public CultureService()
    {
        defaultCulture = new CultureInfo(nameof(IsoCode.De));
    }

    public string GetCurrentIsoCode()
    {
        return GetIso(CultureInfo.CurrentCulture);
    }

    public string GetDefaultIsoCode()
    {
        return GetIso(defaultCulture);
    }

    public string GetTranslation(IEnumerable<IEntityNameTranslation> srcTranslations)
    {
        var currentLang = GetCurrentIsoCode();
        var defaultLang = GetDefaultIsoCode();
        var translationList = srcTranslations?.ToList() ?? new List<IEntityNameTranslation>();

        var translation = translationList.FirstOrDefault(t => t.Language.IsoCode.ToUpperInvariant() == currentLang)
                          ?? translationList.FirstOrDefault(t => t.Language.IsoCode.ToUpperInvariant() == defaultLang);

        return translation?.Name;
    }

    public string GetLanguageTranslation(IEnumerable<LanguageTranslation> srcTranslations)
    {
        var currentLang = GetCurrentIsoCode();
        var defaultLang = GetDefaultIsoCode();
        var translationList = srcTranslations.ToList();

        var translation = translationList.FirstOrDefault(t => string.Equals(t.IsoCode, currentLang, StringComparison.InvariantCultureIgnoreCase))
            ?? translationList.FirstOrDefault(t => string.Equals(t.IsoCode, defaultLang, StringComparison.InvariantCultureIgnoreCase));

        return translation?.Name;
    }

    public NewsItemTranslation? GetTranslation(IEnumerable<NewsItemTranslation> translations)
    {
        var currentLang = GetCurrentIsoCode();
        var defaultLang = GetDefaultIsoCode();

        var list = translations?.ToList() ?? [];

        return list.FirstOrDefault(t => t.Language.IsoCode.ToUpperInvariant() == currentLang)
            ?? list.FirstOrDefault(t => t.Language.IsoCode.ToUpperInvariant() == defaultLang);
    }

    private static string GetIso(CultureInfo info)
    {
        // A fallback is applied when the current culture cannot be resolved.
        // Such as in background services operating within an RHOS environment.
        // https://bkdev.atlassian.net/browse/BKDO-2084
        return string.IsNullOrWhiteSpace(info?.Name) ? "DE" : info.Name[..2].ToUpperInvariant();
    }
}
