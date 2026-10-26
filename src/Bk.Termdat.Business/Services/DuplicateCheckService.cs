using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Bk.Termdat.Business.Repositories;
using Bk.Termdat.Business.Resource.Models;
using Bk.Termdat.Business.Services.Interfaces;

namespace Bk.Termdat.Business.Services;

public class DuplicateCheckService : IDuplicateCheckService
{
    private readonly IDuplicateCheckRepository duplicateCheckRepository;
    private readonly IExcelDataService excelDataService;

    public DuplicateCheckService(IDuplicateCheckRepository duplicateCheckRepository, IExcelDataService excelDataService)
    {
        this.duplicateCheckRepository = duplicateCheckRepository;
        this.excelDataService = excelDataService;
    }

    public async Task<DuplicateCheckResponse> GetDuplicateEntryIds(DuplicateCheckRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var matches = await CheckDuplicatesInternal(request);
        return new DuplicateCheckResponse
        {
            Ids = matches
            .SelectMany(x => new[] { x.EntryId }
            .Concat(x.MatchedEntryIds))
            .ToList()
        };
    }

    public async Task<(string fileName, Stream content)> ExportDuplicateCheckXlsx(DuplicateCheckRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var matches = await CheckDuplicatesInternal(request);

        if (matches.Count == 0)
        {
            return (null, null);
        }

        var (fileName, content) = await excelDataService.CreateDuplicateCheckExport(matches);

        return (fileName, content);
    }

    private async Task<List<DuplicateCheckMatch>> CheckDuplicatesInternal(DuplicateCheckRequest request)
    {
        IReadOnlyList<DuplicateCheckMatch> matches = new List<DuplicateCheckMatch>();

        if (request.CollectionId.HasValue)
        {
            matches = await duplicateCheckRepository.GetByCollectionIdAndLanguageId(request.CollectionId.Value, request.LanguageId);
        }

        if (request.GlossaryId.HasValue)
        {
            matches = await duplicateCheckRepository.GetByGlossaryIdAndLanguageId(request.GlossaryId.Value, request.LanguageId);
        }

        // Filter out reverse matches (B -> A) when the original match (A -> B) already exists

        var processed = new HashSet<(int, int)>();

        var filteredMatches = matches
           .Select(match =>
           {
               // MatchedEntryIds and MatchedKeys are index-aligned (i.e. MatchedKeys[i] is the
               // text that matched MatchedEntryIds[i]), so they must be filtered together to keep
               // that alignment intact.
               var survivingPairs = match.MatchedEntryIds
                   .Select((id, index) => (
                       id,
                       key: index < match.MatchedKeys.Length ? match.MatchedKeys[index] : string.Empty))
                   .Where(pair =>
                   {
                       var dedupeKey = (
                           Math.Min(match.EntryId, pair.id),
                           Math.Max(match.EntryId, pair.id));

                       return processed.Add(dedupeKey);
                   })
                   .ToArray();

               return new DuplicateCheckMatch
               {
                   EntryId = match.EntryId,
                   MatchedEntryIds = survivingPairs.Select(pair => pair.id).ToArray(),
                   MatchedKeys = survivingPairs.Select(pair => pair.key).ToArray()
               };
           })
           .Where(match => match.MatchedEntryIds.Length > 0)
           .ToList();

        return filteredMatches;
    }
}
