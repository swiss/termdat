using System;
using System.Threading;
using System.Threading.Tasks;
using Bk.Termdat.CrossCutting.Configuration;
using Bk.Termdat.Infrastructure.Search;
using Bk.Termdat.Search.Api.BackgroundServices;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using NUnit.Framework;

namespace Bk.Termdat.Search.Api.Tests.BackgroundServices;

[TestFixture]
internal class SearchRefreshBackgroundServiceTests
{
    private SearchRefreshBackgroundService backgroundService;

    private readonly ILogger<SearchRefreshBackgroundService> loggerMock = Substitute.For<ILogger<SearchRefreshBackgroundService>>();
#pragma warning disable NUnit1032
    private readonly ISearchGateway searchGatewayMock = Substitute.For<ISearchGateway>();
#pragma warning restore NUnit1032
    private readonly IOptions<SearchOptions> searchOptionsMock = Substitute.For<IOptions<SearchOptions>>();

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        searchOptionsMock.Value.Returns(new SearchOptions
        {
            RefreshDelay = 5.0f
        });
        backgroundService = new SearchRefreshBackgroundService(loggerMock, searchGatewayMock, searchOptionsMock);
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        backgroundService.Dispose();
    }

    [Test]
    public async Task ExecuteAsync_ShouldMaybeRefresh()
    {
        var maybeRefreshCalled = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        searchGatewayMock
            .When(x => x.MaybeRefreshSearcherManager())
            .Do(_ => maybeRefreshCalled.TrySetResult());

        var token = CancellationToken.None;
        await backgroundService.StartAsync(token);
        await maybeRefreshCalled.Task.WaitAsync(TimeSpan.FromSeconds(1), token);

        searchGatewayMock.Received().MaybeRefreshSearcherManager();
    }
}
