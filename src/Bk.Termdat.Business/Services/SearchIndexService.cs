using System.Threading;
using System.Threading.Tasks;
using Bk.Termdat.Business.Repositories;
using Bk.Termdat.Business.Services.Interfaces;
using Bk.Termdat.CrossCutting.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Bk.Termdat.Business.Services;

public class SearchIndexService : ISearchIndexService
{
    private readonly IEntryRepository entryRepository;
    private readonly ISearchRepository searchRepository;
    private readonly ILogger<SearchIndexService> logger;
    private readonly IOptions<SearchOptions> searchOptions;
    private const int DefaultPageSize = 5000;

    public SearchIndexService(IEntryRepository entryRepository, ISearchRepository searchRepository, ILogger<SearchIndexService> logger, IOptions<SearchOptions> searchOptions)
    {
        this.entryRepository = entryRepository;
        this.searchRepository = searchRepository;
        this.logger = logger;
        this.searchOptions = searchOptions;
    }

    public async Task CreateIndex(CancellationToken stoppingToken)
    {
        var pageSize = searchOptions.Value.IndexPageSize;
        if (pageSize == 0)
        {
            logger.LogInformation("PageSize for indexing not found, using default PageSize = {defaultPageSize}", DefaultPageSize);
            pageSize = DefaultPageSize;
        }

        var total = await entryRepository.Count();
        var pages = (total / pageSize) + 1;
        logger.LogInformation("Adding {Total} entries to index", total);

        for (var i = 0; i < pages; i++)
        {
            if (stoppingToken.IsCancellationRequested)
            {
                break;
            }

            var skip = i * pageSize;
            logger.LogInformation("Loading {Skip}-{TotalSkipped} of {Total}", skip, skip + pageSize, total);
            var entries = entryRepository.GetPageForIndex(skip, pageSize);

            await searchRepository.ImportEntries(entries, stoppingToken);
        }

        logger.LogInformation("Finished adding {Total} entries to index", total);
    }
}
