using System.Linq;
using AutoMapper;
using Bk.Termdat.Business.Domain.Models;
using Bk.Termdat.Business.Services.Interfaces;

namespace Bk.Termdat.Business.Mapping.Resolvers;

public class GlossariesResolver : IValueResolver<Entry, object, string>
{
    private readonly ICultureService cultureService;

    public GlossariesResolver(ICultureService cultureService)
    {
        this.cultureService = cultureService;
    }

    public string Resolve(Entry source, object destination, string destMember, ResolutionContext context)
    {
        return string.Join(", ", source.Glossaries.Select(item => cultureService.GetTranslation(item.Translations)));
    }
}
