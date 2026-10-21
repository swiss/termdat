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

public class EntryService : IEntryService
{
    private readonly IEntryRepository entryRepository;
    private readonly ILanguageValidationService languageValidationService;
    private readonly PublicApiOptions publicApiOptions;
    private readonly IMapper mapper;
    private readonly ILogger<EntryService> logger;

    public EntryService(IEntryRepository entryRepository,
        ILanguageValidationService languageValidationService,
        IOptions<PublicApiOptions> publicApiOptions,
        IMapper mapper,
        ILogger<EntryService> logger)
    {
        this.entryRepository = entryRepository;
        this.languageValidationService = languageValidationService;
        this.publicApiOptions = publicApiOptions.Value;
        this.mapper = mapper;
        this.logger = logger;
    }

    public async Task<IEnumerable<EntryDetail>> GetByIds(int[] entryIds, string inLanguageIsoCode, string outLanguageIsoCode)
    {
        if (entryIds.Length > publicApiOptions.MaximumEntryIds)
        {
            throw new BusinessValidationException(string.Empty,
                $"Maximum number of entry id's allowed is '{publicApiOptions.MaximumEntryIds}'.");
        }
        var inLanguageId = await languageValidationService.ValidateLanguageIsoCode(inLanguageIsoCode);
        logger.LogDebug("Found in language id '{LanguageId}' based on iso code {LanguageIsoCode}", inLanguageId, inLanguageIsoCode);
        int? outLanguageId = !string.IsNullOrEmpty(outLanguageIsoCode) ? (await languageValidationService.ValidateLanguageIsoCode(outLanguageIsoCode)) : null;
        if (outLanguageId.HasValue)
        {
            logger.LogDebug("Found out language id '{LanguageId}' based on iso code {LanguageIsoCode}", outLanguageId, outLanguageIsoCode);
        }

        var entries = (await entryRepository.GetByIdsForApiDetail(entryIds)).ToList();
        logger.LogDebug("Found {Count} entries from the provided {Length} entry id's", entries.Count, entryIds.Length);

        entries = entries
            .Where(e => !e.Deleted
                        && e.Status is Status.Validated
                        && publicApiOptions.DefaultOfficeIds.Contains(e.OfficeId)
                        && e.Collection.ActiveApi).ToList();
        logger.LogDebug(
            "After filtering based on Deleted=false, Status=Validated, OfficeId=[{Ids}] and Collection.ActiveApi=true, we have {Count} entries from the provided {Length} entry id's left",
            string.Join(',', publicApiOptions.DefaultOfficeIds), entries.Count, entryIds.Length);

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
