using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Bk.Termdat.Business.Domain.Models;
using Bk.Termdat.Business.Repositories;
using Bk.Termdat.Business.Resource.Models;
using Bk.Termdat.Business.Resource.Models.Export;
using Bk.Termdat.Business.Services.Interfaces;
using Bk.Termdat.Common.Resources;
using Bk.Termdat.CrossCutting.Exceptions;
using Swiss.FCh.DocumentService.Client;

namespace Bk.Termdat.Business.Services;

public class WatchlistService : IWatchlistService
{
    private readonly IEntryRepository entryRepository;
    private readonly IAuthorizationService authorizationService;
    private readonly IMapper mapper;
    private readonly IDocumentService documentService;

    public WatchlistService(IEntryRepository entryRepository, IAuthorizationService authorizationService, IMapper mapper, IDocumentService documentService)
    {
        this.entryRepository = entryRepository;
        this.authorizationService = authorizationService;
        this.mapper = mapper;
        this.documentService = documentService;
    }

    public async Task<IEnumerable<WatchlistEntry>> GetEntries(IEnumerable<int> entryIds)
    {
        var entries = (await entryRepository.GetByIdsForWatchlist(entryIds)).Where(HasAccessToEntry).ToList();
        if (entries.Count == 0)
        {
            return Array.Empty<WatchlistEntry>();
        }

        return mapper.Map<IEnumerable<WatchlistEntry>>(entries);
    }

    public async Task<Stream> CreateExportPdf(IEnumerable<int> entryIds)
    {
        var entries = (await entryRepository.GetByIdsForExport(entryIds)).ToList();
        if (entries.Count == 0)
        {
            throw new EntityNotFoundException($"No entries for ids='{string.Join(',', entryIds)}' found");
        }

        var export = new WatchlistExport
        {
            TitlePrefix = BackendTexts.WatchlistExportTitlePrefix,
            TitleSuffix = entries.Count > 1 ? BackendTexts.Entries : BackendTexts.Entry,
            Date = DateTime.UtcNow.ToLocalTime().ToString(CultureInfo.CurrentCulture),
            EntryCount = entries.Count,
            Entries = mapper.Map<List<WatchlistExportEntry>>(entries),
        };
        foreach (var item in export.Entries.Select((value, i) => new { value, i }))
        {
            item.value.PositionIndex = item.i + 1;
        }

        var templatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates/Watchlist.docx");
        var stream = await documentService.CreatePdfFromTemplate(templatePath, export, "export");
        return stream;
    }

    private bool HasAccessToEntry(Entry entry)
    {
        return authorizationService.CanAddToWatchList(entry);
    }
}
