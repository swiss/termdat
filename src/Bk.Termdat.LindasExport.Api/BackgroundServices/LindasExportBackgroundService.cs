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

    public LindasExportBackgroundService(ILogger<LindasExportBackgroundService> logger, IServiceProvider serviceProvider, IOptions<LindasExportOptions> lindasExOptions)
    {
        this.logger = logger;
        this.serviceProvider = serviceProvider;
        this.lindasExOptions = lindasExOptions.Value;
    }

    protected override Task ExecuteAsync(CancellationToken ct)
    {
        return Task.Run(async () =>
        {
            logger.LogInformation("{BackgroundService} is starting...", nameof(LindasExportBackgroundService));

            if (!lindasExOptions.Enabled)
            {
                logger.LogInformation("Lindas export is disabled");
                return;
            }

            while (!ct.IsCancellationRequested)
            {
                try
                {
                    logger.LogInformation("Exporting collections to LINDAS...");
                    var timer = Stopwatch.StartNew();

                    using var scope = serviceProvider.CreateScope();
                    var lindasExportService = scope.ServiceProvider.GetRequiredService<ILindasExportService>();
                    await lindasExportService.ExportCollections(ct);

                    timer.Stop();
                    logger.LogInformation("Export collections to LINDAS finished. Duration: {Duration:g}", timer.Elapsed);

                    var process = Process.GetCurrentProcess();
                    var memory = process.PeakWorkingSet64;

                    logger.LogInformation("Process peak memory usage for collection export: {Size}", memory.ToSize(SizeExtension.SizeUnits.MB));

                    await Task.Delay(TimeSpan.FromHours(24), ct);
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Error exporting collections to LINDAS. Error={Message}", e.Message);
                    await Task.Delay(TimeSpan.FromMinutes(1), ct);
                }
            }
        }, ct);
    }
}
