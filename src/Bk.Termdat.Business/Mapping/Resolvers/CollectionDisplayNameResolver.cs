using System;
using AutoMapper;
using Bk.Termdat.Business.Domain.Models;
using Bk.Termdat.Business.Services.Interfaces;

namespace Bk.Termdat.Business.Mapping.Resolvers;

public class CollectionDisplayNameResolver : IValueResolver<Entry, object, string>
{
    private readonly ICultureService cultureService;

    public CollectionDisplayNameResolver(ICultureService cultureService)
    {
        this.cultureService = cultureService;
    }

    public string Resolve(Entry source, object destination, string destMember, ResolutionContext context)
    {
        ArgumentNullException.ThrowIfNull(source);

        return $"{source.Collection.Code} - {cultureService.GetTranslation(source.Collection.Translations)}";
    }
}
