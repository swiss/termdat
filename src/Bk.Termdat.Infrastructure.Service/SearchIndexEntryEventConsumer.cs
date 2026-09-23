using System.Linq;
using System.Threading.Tasks;
using Bk.Termdat.Business.Domain.Projections;
using Bk.Termdat.Business.Messages;
using Bk.Termdat.Business.Repositories;
using Microsoft.Extensions.Logging;

namespace Bk.Termdat.Infrastructure.Service;

public class SearchIndexEntryEventConsumer
{
    private readonly ISearchRepository searchRepository;
    private readonly IEntryRepository entryRepository;
    private readonly ILogger<SearchIndexEntryEventConsumer> logger;

    public SearchIndexEntryEventConsumer(ISearchRepository searchRepository, IEntryRepository entryRepository, ILogger<SearchIndexEntryEventConsumer> logger)
    {
        this.searchRepository = searchRepository;
        this.entryRepository = entryRepository;
        this.logger = logger;
    }

    public Task Consume(SearchIndexEntryAddEvent context)
    {
        var indexEntries = context.IndexEntries as IndexEntry[] ?? context.IndexEntries.ToArray();

        if (!indexEntries.Any())
        {
            return Task.CompletedTask;
        }

        logger.LogInformation("Received message to add entry with id='{EntryId}'", indexEntries[0].EntryId);

        searchRepository.AddEntries(indexEntries);

        return Task.CompletedTask;
    }

    public Task Consume(SearchIndexEntryUpdateEvent context)
    {
        var indexEntries = context.IndexEntries as IndexEntry[] ?? context.IndexEntries.ToArray();

        if (!indexEntries.Any())
        {
            return Task.CompletedTask;
        }

        logger.LogInformation("Received message to update entry with id='{EntryId}'", indexEntries[0].EntryId);

        searchRepository.UpdateEntries(indexEntries);

        return Task.CompletedTask;
    }

    public async Task Consume(SearchIndexEntryUpdateByEntryIdsEvent context)
    {
        var entryIds = context.EntryIds.ToList();

        if (!entryIds.Any())
        {
            return;
        }

        logger.LogInformation("Received message to update all entries of ids='[{EntryIds}]'", string.Join(',', entryIds));

        var indexEntries = await entryRepository.GetByIdsForIndex(entryIds);

        searchRepository.UpdateEntries(indexEntries);
    }

    public Task Consume(SearchIndexEntryDeleteEvent context)
    {
        var entryIds = context.EntryIds as int[] ?? context.EntryIds.ToArray();

        if (!entryIds.Any())
        {
            return Task.CompletedTask;
        }

        logger.LogInformation("Received message to delete entries with ids='[{EntryIds}]'", string.Join(',', entryIds));

        searchRepository.DeleteEntries(entryIds);

        return Task.CompletedTask;
    }
}
