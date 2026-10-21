using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Bk.Termdat.Business.Clients;
using Bk.Termdat.Business.Public.Api.Mapping;
using Bk.Termdat.Business.Public.Api.Resource.Models;
using Bk.Termdat.Business.Repositories;
using Bk.Termdat.CrossCutting.Configuration;
using Bk.Termdat.CrossCutting.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PrivateApiResourceModels = Bk.Termdat.Business.Resource.Models;

namespace Bk.Termdat.Business.Public.Api.Services;

public class SearchService : ISearchService
{
    private readonly IEntryRepository entryRepository;
    private readonly ICollectionRepository collectionRepository;
    private readonly IClassificationRepository classificationRepository;
    private readonly ILanguageValidationService languageValidationService;
    private readonly ISearchApiClient searchApiClient;
    private readonly IMapper mapper;
    private readonly PublicApiOptions publicApiOptions;
    private readonly ILogger<SearchService> logger;

#pragma warning disable S107
    public SearchService(
        IEntryRepository entryRepository,
        ICollectionRepository collectionRepository,
        IClassificationRepository classificationRepository,
        ILanguageValidationService languageValidationService,
        ISearchApiClient searchApiClient,
        IMapper mapper,
        IOptions<PublicApiOptions> publicApiOptions,
        ILogger<SearchService> logger)
#pragma warning restore S107
    {
        this.entryRepository = entryRepository;
        this.collectionRepository = collectionRepository;
        this.classificationRepository = classificationRepository;
        this.languageValidationService = languageValidationService;
        this.searchApiClient = searchApiClient;
        this.mapper = mapper;
        this.publicApiOptions = publicApiOptions.Value;
        this.logger = logger;
    }

    public async Task<IEnumerable<EntryDetail>> Search(SearchParameter parameter)
    {
        await ValidateAndNormalizeCollectionIds(parameter);

        var inLanguageId = await languageValidationService.ValidateLanguageIsoCode(parameter.InLanguageCode);
        logger.LogDebug("Found in language id '{LanguageId}' based on iso code {LanguageCode}", inLanguageId, parameter.InLanguageCode);
        int? outLanguageId = !string.IsNullOrEmpty(parameter.OutLanguageCode) ? (await languageValidationService.ValidateLanguageIsoCode(parameter.OutLanguageCode)) : null;
        if (outLanguageId.HasValue)
        {
            logger.LogDebug("Found out language id '{LanguageId}' based on iso code {LanguageCode}", outLanguageId, parameter.OutLanguageCode);
        }

        var searchFilterParameter = mapper.Map<PrivateApiResourceModels.SearchFilterParameter>(parameter);
        var searchFields = mapper.Map<PrivateApiResourceModels.SearchFields>(parameter.Field);

        searchFilterParameter.SourceLanguageIds = new[] { inLanguageId };
        searchFilterParameter.Offices = publicApiOptions.DefaultOfficeIds;

        if (parameter.ReturnType == ReturnType.Summary)
        {
            return await SearchForSummary(parameter, searchFilterParameter, searchFields, inLanguageId);
        }

        return await SearchForDetail(parameter, searchFilterParameter, searchFields, inLanguageId, outLanguageId);
    }

    private async Task<IEnumerable<EntryDetail>> SearchForDetail(SearchParameter parameter, PrivateApiResourceModels.SearchFilterParameter searchFilterParameter,
        PrivateApiResourceModels.SearchFields searchFields, int inLanguageId, int? outLanguageId)
    {
        var searchResult = await searchApiClient.Search(searchFilterParameter, searchFields);
        logger.LogDebug("Received {TotalEntryCount} results for search for details", searchResult.TotalEntryCount);

        var entries = (await entryRepository.GetByIdsForApiDetail(searchResult.EntryIds)).ToList();

        foreach (var entry in entries)
        {
            entry.Translations = entry.Translations.Where(t =>
                    t.LanguageId == inLanguageId ||
                    (outLanguageId.HasValue && t.LanguageId == outLanguageId))
                .OrderBy(t => t.Language.SortIndex)
                .ThenBy(t => t.Sequence)
                .ToList();
        }

        return mapper.Map<IEnumerable<EntryDetail>>(entries, options =>
        {
            options.Items.Add(PublicApiMappingProfile.LanguageId, inLanguageId);
            options.Items.Add(PublicApiMappingProfile.LanguageIsoCode, parameter.InLanguageCode);
        });
    }

    private async Task<IEnumerable<EntryDetail>> SearchForSummary(SearchParameter parameter, PrivateApiResourceModels.SearchFilterParameter searchFilterParameter,
        PrivateApiResourceModels.SearchFields searchFields, int inLanguageId)
    {
        var searchResult = await searchApiClient.Search(searchFilterParameter, searchFields);
        logger.LogDebug("Received {TotalEntryCount} results for search for summary", searchResult.TotalEntryCount);

        var entries = (await entryRepository.GetByIdsForApiSummary(searchResult.EntryIds)).ToList();

        foreach (var entry in entries)
        {
            entry.Translations = entry.Translations.Where(t =>
                    t.LanguageId == inLanguageId)
                .OrderBy(t => t.Sequence)
                .ToList();
        }

        var entryDetails = mapper.Map<IEnumerable<EntryDetail>>(entries, options =>
        {
            options.Items.Add(PublicApiMappingProfile.LanguageId, inLanguageId);
            options.Items.Add(PublicApiMappingProfile.LanguageIsoCode, parameter.InLanguageCode);
        }).ToList();


        foreach (var entryDetail in entryDetails)
        {
            var languageDetails = entryDetail.LanguageDetails;

            var hits = new List<KeyValuePair<string, string>>();
            ExtractHits(searchFields, hits, languageDetails);

            entryDetail.Hits = hits
                .Where(h => !string.IsNullOrWhiteSpace(h.Value))
                .Select(h => new Dictionary<string, string>(new[] { h }))
                .ToList();
            entryDetail.LanguageDetails = null;
        }

        return entryDetails;
    }

    private static void ExtractHits(PrivateApiResourceModels.SearchFields searchFields, List<KeyValuePair<string, string>> hits, IList<EntryLanguageDetail> languageDetails)
    {
        if (searchFields.Terminus)
        {
            hits.AddRange(languageDetails.Select(ld => CreateKeyValuePair(nameof(ld.Terminus), ld.Terminus)));
        }

        if (searchFields.Name)
        {
            hits.AddRange(languageDetails.Select(ld => CreateKeyValuePair(nameof(ld.Name), ld.Name)));
        }

        if (searchFields.Abbreviation)
        {
            hits.AddRange(languageDetails.Select(ld => CreateKeyValuePair(nameof(ld.Abbreviation), ld.Abbreviation)));
        }

        if (searchFields.Phraseology)
        {
            hits.AddRange(languageDetails.Select(ld => CreateKeyValuePair(nameof(ld.Phraseology), ld.Phraseology)));
        }

        if (searchFields.Definition)
        {
            hits.AddRange(languageDetails.Select(ld => CreateKeyValuePair(nameof(ld.Definition), ld.Definition)));
        }

        if (searchFields.Note)
        {
            hits.AddRange(languageDetails.Select(ld => CreateKeyValuePair(nameof(ld.Note), ld.Note)));
        }

        if (searchFields.Context)
        {
            hits.AddRange(languageDetails.Select(ld => CreateKeyValuePair(nameof(ld.Context), ld.Context)));
        }

        if (searchFields.Source)
        {
            hits.AddRange(languageDetails.Select(ld => CreateKeyValuePair(nameof(ld.AbbreviationSource), ld.AbbreviationSource)));
            hits.AddRange(languageDetails.Select(ld => CreateKeyValuePair(nameof(ld.ContextSource), ld.ContextSource)));
            hits.AddRange(languageDetails.Select(ld => CreateKeyValuePair(nameof(ld.DefinitionSource), ld.DefinitionSource)));
            hits.AddRange(languageDetails.Select(ld => CreateKeyValuePair(nameof(ld.MetadataSource), ld.MetadataSource)));
            hits.AddRange(languageDetails.Select(ld => CreateKeyValuePair(nameof(ld.NoteSource), ld.NoteSource)));
            hits.AddRange(languageDetails.Select(ld => CreateKeyValuePair(nameof(ld.NameSource), ld.NameSource)));
            hits.AddRange(languageDetails.Select(ld => CreateKeyValuePair(nameof(ld.PhraseologySource), ld.PhraseologySource)));
            hits.AddRange(languageDetails.Select(ld => CreateKeyValuePair(nameof(ld.TerminusSource), ld.TerminusSource)));
        }

        if (searchFields.Metadata)
        {
            hits.AddRange(languageDetails.Select(ld => CreateKeyValuePair(nameof(ld.Metadata), ld.Metadata)));
        }

        if (searchFields.Country)
        {
            hits.AddRange(languageDetails.Select(ld => CreateKeyValuePair(nameof(ld.Country), ld.Country)));
        }
    }

    private static KeyValuePair<string, string> CreateKeyValuePair(string name, string value)
    {
        return new KeyValuePair<string, string>(name.ToLowerInvariant(), value);
    }

    private async Task ValidateAndNormalizeCollectionIds(SearchParameter searchParameter)
    {
        var allowedCollections = (await collectionRepository.GetAllActiveApi()).ToList();
        var allowedCollectionIds = allowedCollections.Select(code => code.Id).ToArray();
        var requestedCollections = searchParameter.CollectionIds ?? Array.Empty<int>();
        if (requestedCollections.Except(allowedCollectionIds).Any())
        {
            throw new BusinessValidationException(string.Empty, "The provided collection id's are not supported.");
        }

        var allowedClassifications = (await classificationRepository.GetAll()).ToList();
        var allowedClassificationIds = allowedClassifications.Select(code => code.Id).ToArray();
        var requestedClassifications = searchParameter.ClassificationIds ?? Array.Empty<int>();
        if (requestedClassifications.Except(allowedClassificationIds).Any())
        {
            throw new BusinessValidationException(string.Empty, "The provided classification id's are not supported.");
        }

        var preselectedClassification = allowedClassifications
            .SingleOrDefault(classification => string.Equals(classification.Code, publicApiOptions.DefaultVariaClassification, StringComparison.CurrentCultureIgnoreCase));
        var preselectedCollectionIds = preselectedClassification != null
            ? allowedCollections
                .Where(code => code.ClassificationId == preselectedClassification.Id)
                .Select(code => code.Id)
                .ToList()
            : new List<int>();

        if (!preselectedCollectionIds.Any())
        {
            logger.LogWarning("No collections with classification code '{DefaultVariaClassification}' found", publicApiOptions.DefaultVariaClassification);
        }

        var collectionsIdsFromRequestedClassifications = allowedCollections
            .Where(collection => collection.ClassificationId.HasValue && requestedClassifications.Any(classificationId => classificationId == collection.ClassificationId.Value))
            .Select(code => code.Id);

        var normalizedCollectionIds = requestedCollections
            .Union(collectionsIdsFromRequestedClassifications)
            .Union(preselectedCollectionIds)
            .ToList();

        searchParameter.CollectionIds = normalizedCollectionIds.ToArray();
    }

}
