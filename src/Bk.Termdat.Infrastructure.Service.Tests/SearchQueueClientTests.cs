using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using Bk.Termdat.Business.Domain.Projections;
using Bk.Termdat.CrossCutting.Configuration;
using Bk.Termdat.Infrastructure.Service.RabbitMq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using NSubstitute.ClearExtensions;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using RabbitMQ.Client;

namespace Bk.Termdat.Infrastructure.Service.Tests;

[TestFixture]
internal class SearchQueueClientTests
{
    private SearchQueueClient searchQueueClient;
    private readonly IRabbitMqChannelFactory channelFactory = Substitute.For<IRabbitMqChannelFactory>();
    private readonly IChannel channel = Substitute.For<IChannel>();
    private readonly IOptions<SearchQueueOptions> searchQueueOptionsMock = Substitute.For<IOptions<SearchQueueOptions>>();
    private readonly SearchQueueOptions searchQueueOptions = new() { PublishTimeoutInSeconds = 1000 };
    private readonly Fixture fixture = new();

    [SetUp]
    public void SetUp()
    {
        channelFactory.CreateChannel().Returns(channel);
        searchQueueOptionsMock.Value.Returns(searchQueueOptions);
        searchQueueClient = new SearchQueueClient(channelFactory, searchQueueOptionsMock, Substitute.For<ILogger<SearchQueueClient>>());
    }

    [TearDown]
    public void TearDown()
    {
        searchQueueClient.Dispose();
        channelFactory.ClearSubstitute();
        channel.ClearSubstitute();
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        channel.Dispose();
    }

    [Test]
    public async Task QueueIndexAddCallsPublishEndpoint()
    {
        var indexEntries = fixture.CreateMany<IndexEntry>();

        await searchQueueClient.QueueIndexAdd(indexEntries);

        await channel
            .Received()
            .BasicPublishAsync(
                Arg.Is("termdat.search"),
                Arg.Is("search.index.add"),
                Arg.Is(false),
                Arg.Any<BasicProperties>(),
                Arg.Any<ReadOnlyMemory<byte>>(),
                Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task QueueIndexAddCatchesOperationCanceledException()
    {
        channel
            .BasicPublishAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<bool>(),
                Arg.Any<BasicProperties>(),
                Arg.Any<ReadOnlyMemory<byte>>(),
                Arg.Any<CancellationToken>())
            .ThrowsForAnyArgs(new OperationCanceledException());
        var indexEntries = fixture.CreateMany<IndexEntry>();

        await searchQueueClient.QueueIndexAdd(indexEntries);

        await channel
            .Received()
            .BasicPublishAsync(
                Arg.Is("termdat.search"),
                Arg.Is("search.index.add"),
                Arg.Is(false),
                Arg.Any<BasicProperties>(),
                Arg.Any<ReadOnlyMemory<byte>>(),
                Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task QueueIndexAddCatchesException()
    {
        channel
            .BasicPublishAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<bool>(),
                Arg.Any<BasicProperties>(),
                Arg.Any<ReadOnlyMemory<byte>>(),
                Arg.Any<CancellationToken>())
            .ThrowsForAnyArgs(new Exception());
        var indexEntries = fixture.CreateMany<IndexEntry>();

        await searchQueueClient.QueueIndexAdd(indexEntries);

        await channel
            .Received()
            .BasicPublishAsync(
                Arg.Is("termdat.search"),
                Arg.Is("search.index.add"),
                Arg.Is(false),
                Arg.Any<BasicProperties>(),
                Arg.Any<ReadOnlyMemory<byte>>(),
                Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task QueueIndexUpdateCallsPublishEndpoint()
    {
        var indexEntries = fixture.CreateMany<IndexEntry>();

        await searchQueueClient.QueueIndexUpdate(indexEntries);

        await channel
            .Received()
            .BasicPublishAsync(
                Arg.Is("termdat.search"),
                Arg.Is("search.index.update"),
                Arg.Is(false),
                Arg.Any<BasicProperties>(),
                Arg.Any<ReadOnlyMemory<byte>>(),
                Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task QueueIndexUpdateCatchesOperationCanceledException()
    {
        channel
            .BasicPublishAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<bool>(),
                Arg.Any<BasicProperties>(),
                Arg.Any<ReadOnlyMemory<byte>>(),
                Arg.Any<CancellationToken>())
            .ThrowsForAnyArgs(new OperationCanceledException());
        var indexEntries = fixture.CreateMany<IndexEntry>();

        await searchQueueClient.QueueIndexUpdate(indexEntries);

        await channel
            .Received()
            .BasicPublishAsync(
                Arg.Is("termdat.search"),
                Arg.Is("search.index.update"),
                Arg.Is(false),
                Arg.Any<BasicProperties>(),
                Arg.Any<ReadOnlyMemory<byte>>(),
                Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task QueueIndexUpdateCatchesException()
    {
        channel
            .BasicPublishAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<bool>(),
                Arg.Any<BasicProperties>(),
                Arg.Any<ReadOnlyMemory<byte>>(),
                Arg.Any<CancellationToken>())
            .ThrowsForAnyArgs(new Exception());
        var indexEntries = fixture.CreateMany<IndexEntry>();

        await searchQueueClient.QueueIndexUpdate(indexEntries);

        await channel
            .Received()
            .BasicPublishAsync(
                Arg.Is("termdat.search"),
                Arg.Is("search.index.update"),
                Arg.Is(false),
                Arg.Any<BasicProperties>(),
                Arg.Any<ReadOnlyMemory<byte>>(),
                Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task QueueIndexUpdateByIdsCallsPublishEndpoint()
    {
        var entryIds = fixture.CreateMany<int>();

        await searchQueueClient.QueueIndexUpdateByIds(entryIds);

        await channel
            .Received()
            .BasicPublishAsync(
                Arg.Is("termdat.search"),
                Arg.Is("search.index.update.byids"),
                Arg.Is(false),
                Arg.Any<BasicProperties>(),
                Arg.Any<ReadOnlyMemory<byte>>(),
                Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task QueueIndexUpdateByIdsCatchesOperationCanceledException()
    {
        channel
            .BasicPublishAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<bool>(),
                Arg.Any<BasicProperties>(),
                Arg.Any<ReadOnlyMemory<byte>>(),
                Arg.Any<CancellationToken>())
            .ThrowsForAnyArgs(new OperationCanceledException());
        var entryIds = fixture.CreateMany<int>();

        await searchQueueClient.QueueIndexUpdateByIds(entryIds);

        await channel
            .Received()
            .BasicPublishAsync(
                Arg.Is("termdat.search"),
                Arg.Is("search.index.update.byids"),
                Arg.Is(false),
                Arg.Any<BasicProperties>(),
                Arg.Any<ReadOnlyMemory<byte>>(),
                Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task QueueIndexUpdateByIdsCatchesException()
    {
        channel
            .BasicPublishAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<bool>(),
                Arg.Any<BasicProperties>(),
                Arg.Any<ReadOnlyMemory<byte>>(),
                Arg.Any<CancellationToken>())
            .ThrowsForAnyArgs(new Exception());
        var entryIds = fixture.CreateMany<int>();

        await searchQueueClient.QueueIndexUpdateByIds(entryIds);

        await channel
            .Received()
            .BasicPublishAsync(
                Arg.Is("termdat.search"),
                Arg.Is("search.index.update.byids"),
                Arg.Is(false),
                Arg.Any<BasicProperties>(),
                Arg.Any<ReadOnlyMemory<byte>>(),
                Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task QueueIndexDeleteWithMultipleEntryIdsCallsPublishEndpoint()
    {
        var entryIds = new[] { fixture.Create<int>(), fixture.Create<int>() };

        await searchQueueClient.QueueIndexDelete(entryIds);

        await channel
            .Received()
            .BasicPublishAsync(
                Arg.Is("termdat.search"),
                Arg.Is("search.index.delete"),
                Arg.Is(false),
                Arg.Any<BasicProperties>(),
                Arg.Any<ReadOnlyMemory<byte>>(),
                Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task QueueIndexUpdateByCollectionCallsPublishEndpoint()
    {
        var ids = fixture.CreateMany<int>().ToList();

        await searchQueueClient.QueueIndexUpdateByIds(ids);

        await channel
            .Received()
            .BasicPublishAsync(
                Arg.Is("termdat.search"),
                Arg.Is("search.index.update.byids"),
                Arg.Is(false),
                Arg.Any<BasicProperties>(),
                Arg.Any<ReadOnlyMemory<byte>>(),
                Arg.Any<CancellationToken>());
    }
}
