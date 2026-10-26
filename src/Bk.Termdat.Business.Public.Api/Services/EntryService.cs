using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Bk.Termdat.Business.Domain.Models;
using Bk.Termdat.Business.Public.Api.Mapping;
using Bk.Termdat.Business.Public.Api.Resource.Models;
using Bk.Termdat.Business.Repositories;
using Bk.Termdat.CrossCutting.Configuration;
using Bk.Termdat.CrossCutting.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Bk.Termdat.Business.Public.Api.Services;

/// <summary>
/// Provides entry-related operations for the public API.
/// </summary>
public class EntryService : IEntryService
{
    private readonly IEntryRepository entryRepository;
    private readonly ILanguageValidationService languageValidationService;
    private readonly PublicApiOptions publicApiOptions;
    private readonly IMapper mapper;
    private readonly ILogger<EntryService> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="EntryService"/> class.
    /// </summary>
    /// <param name="entryRepository">The repository used to retrieve entries.</param>
    /// <param name="languageValidationService">The service used to validate language ISO codes.</param>
    /// <param name="publicApiOptions">The configuration options for the public API.</param>
    /// <param name="mapper">The mapper used to convert domain models into API resource models.</param>
    /// <param name="logger">The logger used to record diagnostic information.</param>
    public EntryService(IEntryRepository entryRepository,
        ILanguageValidationService languageValidationService,
        IOptions<PublicApiOptions> publicApiOptions,
        IMapper mapper,
        ILogger<EntryService> logger)
    {
        ArgumentNullException.ThrowIfNull(publicApiOptions);

        this.entryRepository = entryRepository;
        this.languageValidationService = languageValidationService;
        this.publicApiOptions = publicApiOptions.Value;
        this.mapper = mapper;
        this.logger = logger;
    }

    /// <summary>
    /// Retrieves entries by their identifiers and maps them to API detail models.
    /// </summary>
    /// <param name="entryIds">The identifiers of the entries to retrieve.</param>
    /// <param name="inLanguageIsoCode">The ISO code of the input language.</param>
    /// <param name="outLanguageIsoCode">The optional ISO code of the output language.</param>
    /// <returns>The retrieved entries mapped to their API detail representations.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="entryIds"/> is null.
    /// </exception>
    /// <exception cref="BusinessValidationException">
    /// Thrown when the number of requested entry identifiers exceeds the configured maximum.
    /// </exception>
    public async Task<IEnumerable<EntryDetail>> GetByIds(int[] entryIds, string inLanguageIsoCode, string outLanguageIsoCode)
    {
        ArgumentNullException.ThrowIfNull(entryIds);

        if (entryIds.Length > publicApiOptions.MaximumEntryIds)
        {
            throw new BusinessValidationException(string.Empty,
                $"Maximum number of entry id's allowed is '{publicApiOptions.MaximumEntryIds}'.");
        }
        var inLanguageId = await languageValidationService.ValidateLanguageIsoCode(inLanguageIsoCode);

        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug("Found in language id '{LanguageId}' based on iso code {LanguageIsoCode}", inLanguageId, inLanguageIsoCode);
        }

        int? outLanguageId = !string.IsNullOrEmpty(outLanguageIsoCode) ? (await languageValidationService.ValidateLanguageIsoCode(outLanguageIsoCode)) : null;
        if (outLanguageId.HasValue && logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug("Found out language id '{LanguageId}' based on iso code {LanguageIsoCode}", outLanguageId, outLanguageIsoCode);
        }

        var entries = (await entryRepository.GetByIdsForApiDetail(entryIds)).ToList();

        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug("Found {Count} entries from the provided {Length} entry id's", entries.Count, entryIds.Length);
        }

        entries = entries
            .Where(e => !e.Deleted
                        && e.Status is Status.Validated
                        && publicApiOptions.DefaultOfficeIds.Contains(e.OfficeId)
                        && e.Collection.ActiveApi).ToList();

        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug(
            "After filtering based on Deleted=false, Status=Validated, OfficeId=[{Ids}] and Collection.ActiveApi=true, we have {Count} entries from the provided {Length} entry id's left",
            string.Join(',', publicApiOptions.DefaultOfficeIds), entries.Count, entryIds.Length);
        }

        foreach (var entry in entries)
        {
            entry.Translations = entry.Translations
                .Where(t =>
                    t.LanguageId == inLanguageId ||
                    (outLanguageId.HasValue && t.LanguageId == outLanguageId))
                .OrderBy(t => t.Language.SortIndex)
                .ToList();
        }

        return mapper.Map<IEnumerable<EntryDetail>>(entries, options =>
        {
            options.Items.Add(PublicApiMappingProfile.LanguageId, inLanguageId);
            options.Items.Add(PublicApiMappingProfile.LanguageIsoCode, inLanguageIsoCode);
        });
    }
}
