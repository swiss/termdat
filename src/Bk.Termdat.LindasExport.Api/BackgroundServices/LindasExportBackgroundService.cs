using System.Diagnostics;
using Bk.Termdat.Common.Utilities;
using Bk.Termdat.LindasExport.Api.Configuration;
using Bk.Termdat.LindasExport.Api.Interfaces;
using Microsoft.Extensions.Options;

namespace Bk.Termdat.LindasExport.Api.BackgroundServices;

public class LindasExportBackgroundService : BackgroundService
{
    private readonly ILogger<LindasExportBackgroundService> logger;
    private readonly IServiceProvider serviceProvider;
    private readonly LindasExportOptions lindasExOptions;
    private readonly IExportScheduler exportScheduler;
    private readonly TimeProvider timeProvider;

    public LindasExportBackgroundService(
        ILogger<LindasExportBackgroundService> logger,
        IServiceProvider serviceProvider,
        IOptions<LindasExportOptions> lindasExOptions,
        IExportScheduler exportScheduler,
        TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(lindasExOptions);

        this.logger = logger;
        this.serviceProvider = serviceProvider;
        this.lindasExOptions = lindasExOptions.Value;
        this.exportScheduler = exportScheduler;
        this.timeProvider = timeProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("{BackgroundService} is starting...", nameof(LindasExportBackgroundService));
        }

        if (!lindasExOptions.Enabled)
        {
            logger.LogInformation("Lindas export is disabled");
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            var nextRun = exportScheduler.GetNextRun();
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("Next LINDAS export scheduled for {NextRun}", nextRun);
            }

            await Task.Delay(nextRun - timeProvider.GetUtcNow(), timeProvider, stoppingToken);

            try
            {
                logger.LogInformation("Exporting collections to LINDAS...");
                var timer = Stopwatch.StartNew();

                using var scope = serviceProvider.CreateScope();
                var lindasExportService = scope.ServiceProvider.GetRequiredService<ILindasExportService>();
                var result = await lindasExportService.ExportCollections(stoppingToken);

                timer.Stop();
                if (result.IsSuccess)
                {
                    if (logger.IsEnabled(LogLevel.Information))
                    {
                        logger.LogInformation("Export collections to LINDAS finished. Duration: {Duration:g}", timer.Elapsed);
                    }
                }
                else
                {
                    logger.LogError(
                        "LINDAS export completed with failed targets. FailedTargets={FailedTargets} Duration={Duration:g}",
                        string.Join(", ", result.FailedTargets), timer.Elapsed);
                }

                var process = Process.GetCurrentProcess();
                var memory = process.PeakWorkingSet64;

                if (logger.IsEnabled(LogLevel.Information))
                {
                    logger.LogInformation("Process peak memory usage for collection export: {Size}", memory.ToSize(SizeExtension.SizeUnits.MB));
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error exporting collections to LINDAS. Error={Message}", e.Message);
            }
        }
    }
}
