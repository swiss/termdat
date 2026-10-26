using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Bk.Termdat.Business.Clients;
using Bk.Termdat.Business.Repositories;
using Bk.Termdat.Business.Resource.Models;
using Bk.Termdat.Business.Services.Interfaces;
using Bk.Termdat.Common.Resources;
using Bk.Termdat.CrossCutting.Exceptions;

namespace Bk.Termdat.Business.Services;

public class SearchQueryService : ISearchQueryService
{
    private readonly ISearchApiClient searchApiClient;
    private readonly IEntryRepository entryRepository;
    private readonly IMapper mapper;
    private readonly ISearchStatisticService searchStatisticService;
    private readonly IAuthorizationService authorizationService;
    private readonly IEntryNotePatternService entryNotePatternService;

    public SearchQueryService(ISearchApiClient searchApiClient, IEntryRepository entryRepository, IMapper mapper, ISearchStatisticService searchStatisticService,
        IAuthorizationService authorizationService, IEntryNotePatternService entryNotePatternService)
    {
        this.searchApiClient = searchApiClient;
        this.entryRepository = entryRepository;
        this.mapper = mapper;
        this.searchStatisticService = searchStatisticService;
        this.authorizationService = authorizationService;
        this.entryNotePatternService = entryNotePatternService;
    }

    public async Task<SearchPage> Search(SearchQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);

        if (!authorizationService.IsAdmin())
        {
            query.CollectionsExclude = false;
            query.GlossariesExclude = false;
            query.AdminInfosExclude = false;
            query.CreatedByExclude = false;
            query.ModifiedByExclude = false;
            query.ModifiedPeriodExclude = false;
        }

        var searchParams = mapper.Map<SearchFilterParameter>(query);
        var searchFields = mapper.Map<SearchFields>(query);
        var searchResult = new SearchResult();

        var maxIdsForAdminSuperuser = 1000;
        var maxIdsForWriter = 250;
        var maxIdsForPublic = 100;

        // when searching by EntryIds the normal Search call to the API Client is not necessary, as we know, which IDs we want to see.
        if (searchParams.EntryIds != null && searchParams.EntryIds.Length > 0)
        {
            if ((searchParams.EntryIds.Length > maxIdsForAdminSuperuser && (authorizationService.IsSuperUser() || authorizationService.IsAdmin()))
                || (searchParams.EntryIds.Length > maxIdsForWriter && !authorizationService.IsSuperUser() && !authorizationService.IsAdmin())
                || (searchParams.EntryIds.Length > maxIdsForPublic && !authorizationService.IsSuperUser() && !authorizationService.IsAdmin() && !authorizationService.IsWriter()))
            {
                throw new SearchException(string.Format(CultureInfo.InvariantCulture, BackendTexts.MaximumEntryIdsExceededError, searchParams.EntryIds.Length));
            }

            searchResult.EntryIds = searchParams.EntryIds;
        }
        else
        {
            searchResult = await searchApiClient.Search(searchParams, searchFields);
        }

        var entries = authorizationService.IsAdmin() || authorizationService.IsWriter() || authorizationService.IsSuperReader()
            ? (await entryRepository.GetByIdsForSearchResultsAdminOrWriterOrSuperReader(searchResult.EntryIds)).ToList()
            : (await entryRepository.GetByIdsForSearchResults(searchResult.EntryIds)).ToList();

        if (searchParams.EntryIds != null && searchParams.EntryIds.Length > 0)
        {
            searchResult.TotalEntryCount = entries.Count;
        }

        var targetLanguageIdArray = query.TargetLanguageIds?.ToArray();
        var sourceLanguageIdArray = query.SourceLanguageIds?.ToArray();

        foreach (var entry in entries)
        {
            entry.Translations = entry.Translations.Where(t =>
                targetLanguageIdArray == null || targetLanguageIdArray.Length == 0 || targetLanguageIdArray.Contains(t.LanguageId) ||
                sourceLanguageIdArray == null || sourceLanguageIdArray.Length == 0 || sourceLanguageIdArray.Contains(t.LanguageId))
                    .OrderBy(t => t.Language.SortIndex)
                    .ThenBy(t => t.Sequence)
                    .ToList();

            foreach (var detail in entry.Translations)
            {
                (detail.IsObsolete, detail.IsRare, detail.ShouldAvoid) = entryNotePatternService.DetermineEntryNotePatterns(detail.Note);
            }
        }

        if (searchResult.TotalEntryCount == 0 && !string.IsNullOrEmpty(query.Phrase))
        {
            await searchStatisticService.LogUnsuccessfulSearch(searchParams, searchFields);
        }

        var entrySummaries = mapper.Map<IEnumerable<EntrySummary>>(entries);

        return new SearchPage
        {
            TotalEntryCount = searchResult.TotalEntryCount,
            SearchEntries = entrySummaries
        };
    }

    public async Task<IEnumerable<string>> Recommend(string searchPhrase, IEnumerable<int> languageIds = null)
    {
        return await searchApiClient.Recommend(searchPhrase, languageIds);
    }
}
