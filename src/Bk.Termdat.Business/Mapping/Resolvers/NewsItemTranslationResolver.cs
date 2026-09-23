using System.Collections.Generic;
using AutoMapper;
using Bk.Termdat.Business.Domain.Models;
using Bk.Termdat.Business.Services.Interfaces;

namespace Bk.Termdat.Business.Mapping.Resolvers;

public class NewsItemTitleResolver<TSource, TDestination>
    : IMemberValueResolver<TSource, TDestination, IEnumerable<NewsItemTranslation>, string>
{
    private readonly ICultureService cultureService;

    public NewsItemTitleResolver(ICultureService cultureService)
    {
        this.cultureService = cultureService;
    }

    public string Resolve(
        TSource source,
        TDestination destination,
        IEnumerable<NewsItemTranslation> sourceMember,
        string destMember,
        ResolutionContext context)
    {
        var translation = cultureService.GetTranslation(sourceMember);
        return translation?.Title;
    }
}

public class NewsItemTextResolver<TSource, TDestination>
    : IMemberValueResolver<TSource, TDestination, IEnumerable<NewsItemTranslation>, string>
{
    private readonly ICultureService cultureService;

    public NewsItemTextResolver(ICultureService cultureService)
    {
        this.cultureService = cultureService;
    }

    public string Resolve(
        TSource source,
        TDestination destination,
        IEnumerable<NewsItemTranslation> sourceMember,
        string destMember,
        ResolutionContext context)
    {
        var translation = cultureService.GetTranslation(sourceMember);
        return translation?.Text;
    }
}
