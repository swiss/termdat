using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Bk.Termdat.Business.Services.Interfaces;
using Bk.Termdat.Common.Utilities;
using Bk.Termdat.CrossCutting.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Bk.Termdat.Search.Api.BackgroundServices;

public class CreateIndexBackgroundService : BackgroundService
{
    private readonly IServiceProvider serviceProvider;
    private readonly ILogger<CreateIndexBackgroundService> logger;
    private readonly SearchOptions searchOptions;

    public CreateIndexBackgroundService(ILogger<CreateIndexBackgroundService> logger, IServiceProvider serviceProvider, IOptions<SearchOptions> searchOptions)
    {
        this.logger = logger;
        this.serviceProvider = serviceProvider;
        this.searchOptions = searchOptions.Value;
    }

    [SuppressMessage("Microsoft.Design", "CA1031: Do not catch general exception types", Justification = "Exceptions should not stop the background worker.")]
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            if (!searchOptions.RecreateIndexOnStartUp)
            {
                logger.LogInformation("Skipping index creating in background service ({Name}=false)", nameof(searchOptions.RecreateIndexOnStartUp));
                return;
            }

            logger.LogInformation("Creating index in background service...");
            var timer = Stopwatch.StartNew();

            using var scope = serviceProvider.CreateScope();
            var searchService = scope.ServiceProvider.GetRequiredService<ISearchIndexService>();

            await searchService.CreateIndex(stoppingToken);

            timer.Stop();
            logger.LogInformation("Adding data to index took {Elapsed:g}", timer.Elapsed);

            var process = Process.GetCurrentProcess();
            var memory = process.PeakWorkingSet64;

            logger.LogInformation("Process peak memory usage for create index: {Size}", memory.ToSize(SizeExtension.SizeUnits.MB));
        }
        catch (TaskCanceledException)
        {
            logger.LogInformation("SearchIndexWorker stopped by request");
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("SearchIndexWorker stopped by request");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error on creating index");
            if (!(searchOptions.ContinueOnError ?? false))
            {
                throw;
            }
        }
    }
}
