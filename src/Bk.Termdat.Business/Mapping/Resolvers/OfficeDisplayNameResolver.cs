using AutoMapper;
using Bk.Termdat.Business.Domain.Models;
using Bk.Termdat.Business.Services.Interfaces;

namespace Bk.Termdat.Business.Mapping.Resolvers;

public class OfficeDisplayNameResolver : IValueResolver<Entry, object, string>
{
    private readonly ICultureService cultureService;

    public OfficeDisplayNameResolver(ICultureService cultureService)
    {
        this.cultureService = cultureService;
    }

    public string Resolve(Entry source, object destination, string destMember, ResolutionContext context)
    {
        return $"{source.Office.Code} - {cultureService.GetTranslation(source.Office.Translations)}";
    }
}
