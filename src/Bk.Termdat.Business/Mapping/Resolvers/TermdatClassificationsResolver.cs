using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Bk.Termdat.Business.Domain.Models;
using Bk.Termdat.Business.Resource.Models.Export;
using Bk.Termdat.Business.Services.Interfaces;

namespace Bk.Termdat.Business.Mapping.Resolvers;

public class TermdatClassificationsResolver : IValueResolver<Entry, object, List<WatchlistExportTermdatClassification>>
{
    private readonly ICultureService cultureService;

    public TermdatClassificationsResolver(ICultureService cultureService)
    {
        this.cultureService = cultureService;
    }

    public List<WatchlistExportTermdatClassification> Resolve(
        Entry source,
        object destination,
        List<WatchlistExportTermdatClassification> destMember,
        ResolutionContext context)
    {
        return source.Subjects.Select(subject => new WatchlistExportTermdatClassification
        {
            Subject = cultureService.GetTranslation(subject.Translations),
            Domains = source.Domains
                .Where(item => item.SubjectId == subject.Id)
                .Select(item => cultureService.GetTranslation(item.Translations))
                .ToList()
        }).ToList();
    }
}
