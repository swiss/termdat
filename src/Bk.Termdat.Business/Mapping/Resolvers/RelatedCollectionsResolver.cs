using System;
using System.Linq;
using AutoMapper;
using Bk.Termdat.Business.Domain.Models;
using Bk.Termdat.Business.Services.Interfaces;

namespace Bk.Termdat.Business.Mapping.Resolvers;

public class RelatedCollectionsResolver : IValueResolver<Entry, object, string>
{
    private readonly ICultureService cultureService;

    public RelatedCollectionsResolver(ICultureService cultureService)
    {
        this.cultureService = cultureService;
    }

    public string Resolve(Entry source, object destination, string destMember, ResolutionContext context)
    {
        ArgumentNullException.ThrowIfNull(source);

        return string.Join(", ", source.RelatedCollections.Select(item => cultureService.GetTranslation(item.Translations)));
    }
}
