using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Bk.Termdat.Business.Domain.Models;
using Bk.Termdat.Business.Resource.Models.Export;
using Bk.Termdat.Business.Services.Interfaces;

namespace Bk.Termdat.Business.Mapping.Resolvers;

public class TermdatClassificationsResolver : IValueResolver<Entry, WatchlistExportEntry, IList<WatchlistExportTermdatClassification>>
{
    private readonly ICultureService cultureService;

    public TermdatClassificationsResolver(ICultureService cultureService)
    {
        this.cultureService = cultureService;
    }

    public IList<WatchlistExportTermdatClassification> Resolve(
        Entry source,
        WatchlistExportEntry destination,
        IList<WatchlistExportTermdatClassification> destMember,
        ResolutionContext context)
    {
        ArgumentNullException.ThrowIfNull(source);

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
