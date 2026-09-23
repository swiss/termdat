using System;
using System.Threading;
using System.Threading.Tasks;
using Bk.Termdat.Business.Services.Interfaces;
using Bk.Termdat.CrossCutting.Configuration;
using Bk.Termdat.Search.Api.BackgroundServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using NUnit.Framework;

namespace Bk.Termdat.Search.Api.Tests.BackgroundServices;

[TestFixture]
internal class CreateIndexBackgroundServiceTests
{
    private BackgroundService backgroundService;
#pragma warning disable NUnit1032
    private IServiceProvider serviceProvider;
#pragma warning restore NUnit1032

    private readonly ILogger<CreateIndexBackgroundService> loggerMock = Substitute.For<ILogger<CreateIndexBackgroundService>>();
    private readonly ISearchIndexService searchIndexServiceMock = Substitute.For<ISearchIndexService>();
    private readonly IOptions<SearchOptions> searchOptionsMock = Substitute.For<IOptions<SearchOptions>>();

    [SetUp]
    public void Setup()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddScoped(_ => searchIndexServiceMock);
        serviceProvider = serviceCollection.BuildServiceProvider();
        searchOptionsMock.Value.Returns(new SearchOptions { ContinueOnError = false, RecreateIndexOnStartUp = true });

        backgroundService = new CreateIndexBackgroundService(loggerMock, serviceProvider, searchOptionsMock);
    }

    [TearDown]
    public void TearDown()
    {
        backgroundService.Dispose();
    }

    [Test]
    public async Task ExecuteAsync_ShouldRunCreateIndex()
    {
        var source = new CancellationTokenSource();
        await backgroundService.StartAsync(source.Token);

        // INFO: wait x seconds to let the CreateIndexBackgroundService.CreateIndex complete
        await Task.Delay(TimeSpan.FromSeconds(3));

        await searchIndexServiceMock.ReceivedWithAnyArgs().CreateIndex(source.Token);
    }

    [Test]
    public async Task ExecuteAsync_ShouldNotCreateIndexWhenRecreateIndexOnStartUpIsFalse()
    {
        searchOptionsMock.Value.Returns(new SearchOptions { ContinueOnError = false, RecreateIndexOnStartUp = false });
        backgroundService = new CreateIndexBackgroundService(loggerMock, serviceProvider, searchOptionsMock);

        var source = new CancellationTokenSource();
        await backgroundService.StartAsync(source.Token);

        // INFO: wait x seconds to let the CreateIndexBackgroundService.CreateIndex complete
        await Task.Delay(TimeSpan.FromSeconds(3));

        await searchIndexServiceMock.ReceivedWithAnyArgs(0).CreateIndex(source.Token);
    }
}
