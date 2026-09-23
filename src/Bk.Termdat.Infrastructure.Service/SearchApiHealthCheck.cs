using System;
using System.Threading;
using System.Threading.Tasks;
using Bk.Termdat.Business.Clients;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Bk.Termdat.Infrastructure.Service;

public class SearchApiHealthCheck : IHealthCheck
{
    private readonly ISearchApiClient searchApiClient;

    public SearchApiHealthCheck(ISearchApiClient searchApiClient)
    {
        this.searchApiClient = searchApiClient;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var isAlive = await searchApiClient.IsAlive();
            return isAlive
                ? HealthCheckResult.Healthy("Search API connection is healthy")
                : HealthCheckResult.Unhealthy("Unable to connect to the Search API");
        }
        catch (Exception e)
        {
            return HealthCheckResult.Unhealthy("An exception occurred while checking Search API connection", e);
        }
    }
}
