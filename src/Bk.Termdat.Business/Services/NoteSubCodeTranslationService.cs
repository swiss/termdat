using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Bk.Termdat.Business.Repositories;
using Bk.Termdat.Business.Resource.Models;
using Bk.Termdat.Business.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace Bk.Termdat.Business.Services;

public class NoteSubCodeTranslationService : INoteSubCodeTranslationService
{
    private const int EnLanguageId = 3;

    private static readonly Regex noteRegex =
        new(@"(?<code>[A-Z]{3})\s*:\s*(?<text>.*?)(?=\s*;\s*[A-Z]{3}\s*:|$)",
            RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));

    private readonly INoteSubCodeRepository repository;
    private readonly IMemoryCache cache;
    private readonly ICultureService cultureService;

    public NoteSubCodeTranslationService(
        INoteSubCodeRepository repository,
        IMemoryCache cache,
        ICultureService cultureService)
    {
        this.repository = repository;
        this.cache = cache;
        this.cultureService = cultureService;
    }

    public IList<NoteSubCodeTooltip> GetTooltips(string note)
    {
        if (string.IsNullOrWhiteSpace(note))
        {
            return new List<NoteSubCodeTooltip>();
        }

        var translations = GetTranslations();
        var languageIsoToId = GetLanguageIsoToId();
        var currentIso = NormalizeIsoCode(cultureService.GetCurrentIsoCode());
        languageIsoToId.TryGetValue(currentIso, out var languageId);

        var result = new List<NoteSubCodeTooltip>();

        foreach (Match match in noteRegex.Matches(note))
        {
            var code = match.Groups["code"].Value.Trim().ToUpperInvariant();

            translations.TryGetValue((code, languageId), out var tooltip);
            tooltip ??= translations.TryGetValue((code, EnLanguageId), out var enTooltip) ? enTooltip : null;

            result.Add(new NoteSubCodeTooltip
            {
                Code = code,
                Tooltip = tooltip
            });
        }

        return result;
    }

    private Dictionary<(string Code, int LanguageId), string> GetTranslations()
    {
        const string cacheKey = "NoteSubCodeTranslations:translations";

        return cache.GetOrCreate(cacheKey, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);

            return repository.GetTranslations().ToDictionary(
                x => (x.NoteSubCode.Code.ToUpperInvariant(), x.LanguageId),
                x => x.Name);
        });
    }

    private Dictionary<string, int> GetLanguageIsoToId()
    {
        const string cacheKey = "NoteSubCodeTranslations:languageIsoToId";

        return cache.GetOrCreate(cacheKey, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);

            var map = new Dictionary<string, int>();
            foreach (var translation in repository.GetTranslations())
            {
                var language = translation.Language;
                if (language == null || string.IsNullOrWhiteSpace(language.IsoCode))
                {
                    continue;
                }

                map[NormalizeIsoCode(language.IsoCode)] = language.Id;
            }

            return map;
        });
    }

    private static string NormalizeIsoCode(string isoCode)
    {
        return string.IsNullOrWhiteSpace(isoCode) ? string.Empty : isoCode[..2].ToUpperInvariant();
    }
}
