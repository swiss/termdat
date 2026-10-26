using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Bk.Termdat.CrossCutting.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Bk.Termdat.Business.Public.Api.Services;

public class AnalyticsService : IAnalyticsService
{
    private readonly ILogger<AnalyticsService> logger;
    private readonly AnalyticsOptions analyticsOptions;
    private readonly IHttpClientFactory httpClientFactory;

    public AnalyticsService(ILogger<AnalyticsService> logger, IOptions<AnalyticsOptions> analyticsOptions, IHttpClientFactory httpClientFactory)
    {
        ArgumentNullException.ThrowIfNull(analyticsOptions);

        this.logger = logger;
        this.httpClientFactory = httpClientFactory;
        this.analyticsOptions = analyticsOptions.Value;
    }
    public async Task TrackEvent(string category, string action, string? name = null)
    {
        if (!analyticsOptions.Enabled)
        {
            return;
        }

        try
        {
            using var client = httpClientFactory.CreateClient();

            var parameters = HttpUtility.ParseQueryString(string.Empty);
            parameters["idsite"] = analyticsOptions.SiteId;
            parameters["rec"] = "1";

            parameters["e_c"] = category;
            parameters["e_a"] = action;

            if (!string.IsNullOrWhiteSpace(name))
            {
                parameters["e_n"] = name;
            }

            using var response = await client.GetAsync($"{analyticsOptions.Url}?{parameters}");
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Failed to track analytics event. HttpError:{response.StatusCode}");
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error tracking event: Category={Category} Action={Action} Name={Name}", category, action, name);
        }
    }
}
