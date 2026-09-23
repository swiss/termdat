using System.Collections.Generic;
using AutoMapper;
using Bk.Termdat.Business.Domain.Models;
using Bk.Termdat.Business.Services.Interfaces;

namespace Bk.Termdat.Business.Mapping.Resolvers;

public class TranslationResolver<TSource, TDestination> : IMemberValueResolver<TSource, TDestination, IEnumerable<IEntityNameTranslation>, string>
{
    private readonly ICultureService cultureService;

    public TranslationResolver(ICultureService cultureService)
    {
        this.cultureService = cultureService;
    }

    public string Resolve(
        TSource source,
        TDestination destination,
        IEnumerable<IEntityNameTranslation> sourceMember,
        string destMember,
        ResolutionContext context)
    {
        return cultureService.GetTranslation(sourceMember);
    }
}
