using AutoMapper;
using Bk.Termdat.Business.Domain.Models;
using Bk.Termdat.Business.Public.Api.Resource.Models;
using Bk.Termdat.Business.Services.Interfaces;

namespace Bk.Termdat.Business.Public.Api.Mapping.Resolvers;

public class EntryDetailUrlResolver : IValueResolver<Entry, EntryDetail, string>
{
    private readonly ITermdatUrlService termdatUrlService;

    public EntryDetailUrlResolver(ITermdatUrlService termdatUrlService)
    {
        this.termdatUrlService = termdatUrlService;
    }

    public string Resolve(Entry source, EntryDetail destination, string destMember, ResolutionContext context)
    {
        return termdatUrlService.GetTermdatUrl(source.Id);
    }
}
