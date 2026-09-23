using System.Linq;
using AutoMapper;
using Bk.Termdat.Business.Services.Interfaces;
using DomainModels = Bk.Termdat.Business.Domain.Models;
using ResourceModels = Bk.Termdat.Business.Resource.Models;

namespace Bk.Termdat.Business.Mapping.Resolvers;

public class EntryWatchlistValueResolver : IValueResolver<DomainModels.Entry, ResourceModels.WatchlistEntry, string>
{
    private readonly ICultureService cultureService;

    public EntryWatchlistValueResolver(ICultureService cultureService)
    {
        this.cultureService = cultureService;
    }

    public string Resolve(DomainModels.Entry source, ResourceModels.WatchlistEntry destination, string destMember, ResolutionContext context)
    {
        var currentLang = cultureService.GetCurrentIsoCode();
        var entryLanguage = source.Translations
            .OrderBy(t => t.Sequence)
            .FirstOrDefault(t => t.Language.IsoCode == currentLang);

        if (entryLanguage == null)
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
