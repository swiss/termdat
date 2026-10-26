using System;
using System.Threading;
using System.Threading.Tasks;
using Bk.Termdat.CrossCutting.Configuration;
using Bk.Termdat.Infrastructure.Search;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Bk.Termdat.Search.Api.BackgroundServices;

public class SearchRefreshBackgroundService : BackgroundService
{
    private readonly ILogger<SearchRefreshBackgroundService> logger;
    private readonly ISearchGateway searchGateway;
    private readonly SearchOptions options;

    public SearchRefreshBackgroundService(ILogger<SearchRefreshBackgroundService> logger, ISearchGateway searchGateway, IOptions<SearchOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);

        this.logger = logger;
        this.searchGateway = searchGateway;
        this.options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                searchGateway.MaybeRefreshSearcherManager();
                logger.LogInformation("SearcherManager was refreshed");

                await Task.Delay(TimeSpan.FromMinutes(options.RefreshDelay), stoppingToken).ConfigureAwait(false);
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error refreshing SearcherManager {Message}", e.Message);
        }
    }
}
