using System;
using System.Threading;
using System.Threading.Tasks;
using Bk.Termdat.Business.Services.Interfaces;
using Bk.Termdat.CrossCutting.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Bk.Termdat.Private.Api.HostedServices;

public class EntryUnlockBackgroundService : BackgroundService
{
    private readonly IBackgroundServiceScheduler backgroundServiceScheduler;
    private readonly ILogger<EntryUnlockBackgroundService> logger;
    private readonly IServiceProvider services;
    private readonly EntryUnlockOptions entryUnlockOptions;

    public EntryUnlockBackgroundService(IBackgroundServiceScheduler backgroundServiceScheduler, ILogger<EntryUnlockBackgroundService> logger, IServiceProvider services, IOptions<EntryUnlockOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);

        this.backgroundServiceScheduler = backgroundServiceScheduler;
        this.logger = logger;
        this.services = services;
        entryUnlockOptions = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation($"{nameof(EntryUnlockBackgroundService)} is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            var nextInterval = backgroundServiceScheduler.GetNextRun(entryUnlockOptions.IntervalInSeconds);
            await Task.Delay(nextInterval, stoppingToken);

            await UnlockEntries();
        }
    }

    private async Task UnlockEntries()
    {
        try
        {
            logger.LogInformation("Start entry unlock service");

            using var scope = services.CreateScope();
            var entryService = scope.ServiceProvider.GetRequiredService<IEntryService>();

            await entryService.UnlockExpiredEntries();

            logger.LogInformation("Entry unlock service task ended");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error on unlocking entries");
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation($"{nameof(EntryUnlockBackgroundService)} is stopping.");

        await base.StopAsync(cancellationToken);
    }

}
