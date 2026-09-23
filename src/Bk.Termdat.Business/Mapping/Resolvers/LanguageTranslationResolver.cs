using System.Collections.Generic;
using AutoMapper;
using Bk.Termdat.Business.Domain.Models;
using Bk.Termdat.Business.Services.Interfaces;

namespace Bk.Termdat.Business.Mapping.Resolvers;

public class LanguageTranslationResolver<TSource, TDestination> : IMemberValueResolver<TSource, TDestination, IEnumerable<LanguageTranslation>, string>
{
    private readonly ICultureService cultureService;

    public LanguageTranslationResolver(ICultureService cultureService)
    {
        this.cultureService = cultureService;
    }

    public string Resolve(
        TSource source,
        TDestination destination,
        IEnumerable<LanguageTranslation> sourceMember,
        string destMember,
        ResolutionContext context)
    {
        return cultureService.GetLanguageTranslation(sourceMember);
    }
}
