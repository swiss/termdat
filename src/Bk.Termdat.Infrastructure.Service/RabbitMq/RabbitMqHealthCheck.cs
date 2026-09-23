using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Bk.Termdat.Infrastructure.Service.RabbitMq;

public class RabbitMqHealthCheck : IHealthCheck
{
    private readonly IRabbitMqChannelFactory channelFactory;

    public RabbitMqHealthCheck(IRabbitMqChannelFactory channelFactory)
    {
        this.channelFactory = channelFactory;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            await using var channel = await channelFactory.CreateChannel();
            return channel.IsOpen
                ? HealthCheckResult.Healthy("RabbitMq connection is healthy")
                : HealthCheckResult.Unhealthy("RabbitMq connection is unhealthy");
        }
        catch (Exception e)
        {
            return HealthCheckResult.Unhealthy("An exception occurred while checking RabbitMq connection", e);
        }
    }
}
