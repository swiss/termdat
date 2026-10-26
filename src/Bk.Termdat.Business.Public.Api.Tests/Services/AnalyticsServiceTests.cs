using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Bk.Termdat.Business.Public.Api.Services;
using Bk.Termdat.CrossCutting.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using NSubstitute.ClearExtensions;
using NUnit.Framework;
using RichardSzalay.MockHttp;

namespace Bk.Termdat.Business.Public.Api.Tests.Services;

[TestFixture]
internal class AnalyticsServiceTests
{
    private readonly ILogger<AnalyticsService> logger = NullLogger<AnalyticsService>.Instance;
    private readonly IHttpClientFactory httpClientFactory = Substitute.For<IHttpClientFactory>();
    private readonly AnalyticsOptions analyticsOptions = new() { Enabled = true, SiteId = "1", Url = "https://example.com/track" };

    private HttpClient httpClient;
    private MockHttpMessageHandler mockHttpMessageHandler;

    private AnalyticsService analyticsService;

    [SetUp]
    public void SetUp()
    {
        var options = Options.Create(analyticsOptions);

        mockHttpMessageHandler = new MockHttpMessageHandler();
        httpClient = mockHttpMessageHandler.ToHttpClient();
        httpClient.BaseAddress = new Uri(options.Value.Url);

#pragma warning disable CA2000
        httpClientFactory.CreateClient().Returns(httpClient);
#pragma warning restore CA2000

        analyticsService = new AnalyticsService(logger, options, httpClientFactory);
    }

    [TearDown]
    public void TearDown()
    {
        httpClientFactory.ClearSubstitute();
        httpClient.Dispose();
        mockHttpMessageHandler.Dispose();
    }

    [Test]
    public async Task TrackEvent_ShouldSendRequest()
    {
        var mockRequest = mockHttpMessageHandler
            .When(HttpMethod.Get, $"{analyticsOptions.Url}*")
            .Respond(HttpStatusCode.OK);

        await analyticsService.TrackEvent("TestCategory", "TestAction", "TestName");

        Assert.That(mockHttpMessageHandler.GetMatchCount(mockRequest), Is.EqualTo(1));
    }
}
