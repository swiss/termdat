using System.Linq;
using AutoMapper;
using Bk.Termdat.Business.Domain.Models;
using Bk.Termdat.Business.Services.Interfaces;

namespace Bk.Termdat.Business.Mapping.Resolvers;

public class WatchlistExportEntryValueResolver : IValueResolver<Entry, object, string>
{
    private readonly ICultureService cultureService;

    public WatchlistExportEntryValueResolver(ICultureService cultureService)
    {
        this.cultureService = cultureService;
    }

    public string Resolve(Entry source, object destination, string destMember, ResolutionContext context)
    {
        var currentLang = cultureService.GetCurrentIsoCode();
        var entryLanguage = source.Translations.OrderBy(t => t.Sequence).FirstOrDefault(t => t.Language.IsoCode == currentLang);
        if (entryLanguage is null)
        {
            return null;
        }

        if (!string.IsNullOrWhiteSpace(entryLanguage.Terminus))
        {
            return entryLanguage.Terminus;
        }

        if (!string.IsNullOrWhiteSpace(entryLanguage.Name))
        {
            return entryLanguage.Name;
        }

        if (!string.IsNullOrWhiteSpace(entryLanguage.Abbreviation))
        {
            return entryLanguage.Abbreviation;
        }

        return entryLanguage.Phraseology;
    }
}
