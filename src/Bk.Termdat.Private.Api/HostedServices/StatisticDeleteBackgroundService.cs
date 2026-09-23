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

public class StatisticDeleteBackgroundService : BackgroundService
{
    private readonly IBackgroundServiceScheduler backgroundServiceScheduler;
    private readonly ILogger<StatisticDeleteBackgroundService> logger;
    private readonly IServiceProvider services;
    private readonly StatisticDeleteOptions statisticDeleteOptions;

    public StatisticDeleteBackgroundService(IBackgroundServiceScheduler backgroundServiceScheduler, ILogger<StatisticDeleteBackgroundService> logger, IServiceProvider services, IOptions<StatisticDeleteOptions> options)
    {
        this.backgroundServiceScheduler = backgroundServiceScheduler;
        this.logger = logger;
        this.services = services;
        statisticDeleteOptions = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation($"{nameof(StatisticDeleteBackgroundService)} is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            var nextInterval = backgroundServiceScheduler.GetNextRun(statisticDeleteOptions.IntervalInSeconds);
            await Task.Delay(nextInterval, stoppingToken);

            await DeleteStatistics();
        }
    }

    private async Task DeleteStatistics()
    {
        try
        {
            logger.LogInformation("Start delete statistic service");

            using var scope = services.CreateScope();
            var statisticService = scope.ServiceProvider.GetRequiredService<IStatisticService>();

            await statisticService.DeleteObsoleteStatistics();

            logger.LogInformation("Delete statistic service task ended");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error on deleting statistics");
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation($"{nameof(StatisticDeleteBackgroundService)} is stopping.");

        await base.StopAsync(cancellationToken);
    }
}
