using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Bk.Termdat.Private.Api.HostedServices;

public class QueuedBackgroundService : BackgroundService
{
    private readonly IBackgroundTaskQueue backgroundTaskQueue;
    private readonly ILogger<QueuedBackgroundService> logger;
    private readonly IServiceProvider serviceProvider;

    public QueuedBackgroundService(IBackgroundTaskQueue backgroundTaskQueue,
        ILogger<QueuedBackgroundService> logger, IServiceProvider serviceProvider)
    {
        this.backgroundTaskQueue = backgroundTaskQueue;
        this.logger = logger;
        this.serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("{ServiceName} is running", nameof(QueuedBackgroundService));

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var workItem = await backgroundTaskQueue.DequeueBackgroundWorkItemAsync(cancellationToken);

                await workItem(cancellationToken, serviceProvider);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "{ServiceName}: Error occurred executing the workItem function",
                    nameof(QueuedBackgroundService));
            }
        }
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("{ServiceName} is stopping", nameof(QueuedBackgroundService));

        await base.StopAsync(stoppingToken);
    }
}
