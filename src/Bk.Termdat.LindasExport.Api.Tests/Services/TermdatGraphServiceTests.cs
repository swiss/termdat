using Bk.Termdat.LindasExport.Api.Configuration;
using Bk.Termdat.LindasExport.Api.Constants;
using Bk.Termdat.LindasExport.Api.Interfaces;
using Bk.Termdat.LindasExport.Api.Models;
using Bk.Termdat.LindasExport.Api.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using VDS.RDF;
using VDS.RDF.Storage;

namespace Bk.Termdat.LindasExport.Api.Tests.Services;

[TestFixture]
internal class TermdatGraphServiceTests
{
    private const string Target = "TestTarget";
    private readonly ILogger<TermdatGraphService> logger = NullLogger<TermdatGraphService>.Instance;
    private readonly IAsyncStorageProvider storageProvider = Substitute.For<IAsyncStorageProvider>();
    private readonly IOptions<TermdatGraphOptions> graphOptions = Substitute.For<IOptions<TermdatGraphOptions>>();
    private readonly IOptions<LindasExportOptions> exportOptions = Substitute.For<IOptions<LindasExportOptions>>();
    private readonly ISparqlClientFactory sparqlClientFactory = Substitute.For<ISparqlClientFactory>();
    private readonly ILindasOperationExecutor operationExecutor = Substitute.For<ILindasOperationExecutor>();
    private readonly IChunkDelay chunkDelay = Substitute.For<IChunkDelay>();
    private TermdatGraphService graphService = null!;
    private static readonly CancellationToken ct = CancellationToken.None;

    [SetUp]
    public void SetUp()
    {
        graphOptions.Value.Returns(new TermdatGraphOptions { ChunkSize = 1_000 });
        exportOptions.Value.Returns(new LindasExportOptions
        {
            AddCubeLinkData = false,
            RetryBaseDelayMinutes = 0,
            MinimumChunkDelayMs = 0,
            MaximumChunkDelayMs = 0,
            ExportTime = default,
            MaximumAttempts = 0
        });
        sparqlClientFactory.GetStorageProviders().Returns(
            new Dictionary<string, IAsyncStorageProvider> { [Target] = storageProvider });
        operationExecutor
            .Execute(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Func<Task>>(), Arg.Any<CancellationToken>())
            .Returns(call => call.ArgAt<Func<Task>>(2)());

        graphService = new TermdatGraphService(
            logger, exportOptions, graphOptions, sparqlClientFactory, operationExecutor, chunkDelay);
    }

    [TearDown]
    public void TearDown()
    {
        sparqlClientFactory.ClearSubstitute();
        storageProvider.ClearSubstitute();
        operationExecutor.ClearSubstitute();
        chunkDelay.ClearSubstitute();
    }

    [OneTimeTearDown]
    public void Cleanup()
    {
        storageProvider.Dispose();
    }

    [Test]
    public void Targets_ShouldReturnConfiguredTargets()
    {
        Assert.That(graphService.GetTargets(), Is.EqualTo(new[] { Target }));
    }

    [Test]
    public async Task DeleteGraph_ShouldCallTargetStorageProvider()
    {
        await graphService.DeleteGraph(Target, ct);

        await storageProvider.Received(1).DeleteGraphAsync(Uris.TermdatGraph, ct);
    }

    [Test]
    public void AddCollection_ForNullCollection_ShouldThrowArgumentNullException()
    {
        Assert.That(
            () => graphService.AddCollection(Target, null!, ct),
            Throws.InstanceOf<ArgumentNullException>());
    }

    [Test]
    public async Task AddDataSet_ShouldCallTargetStorageProvider()
    {
        storageProvider
            .When(x => x.UpdateGraphAsync(Arg.Any<string>(), Arg.Any<IEnumerable<Triple>>(), [], ct))
            .Do(x => Assert.That(x.ArgAt<IEnumerable<Triple>>(1).Count(), Is.EqualTo(26)));

        await graphService.AddDataSet(Target, ct);

        await storageProvider.Received(1)
            .UpdateGraphAsync(Arg.Any<string>(), Arg.Any<IEnumerable<Triple>>(), [], ct);
    }

    [Test]
    public async Task AddDataSet_WithEnabledCubeLinkData_ShouldAddAdditionalData()
    {
        exportOptions.Value.Returns(new LindasExportOptions
        {
            AddCubeLinkData = true,
            RetryBaseDelayMinutes = 0,
            MinimumChunkDelayMs = 0,
            MaximumChunkDelayMs = 0,
            ExportTime = default,
            MaximumAttempts = 0
        });
        var sut = new TermdatGraphService(
            logger, exportOptions, graphOptions, sparqlClientFactory, operationExecutor, chunkDelay);
        storageProvider
            .When(x => x.UpdateGraphAsync(Arg.Any<string>(), Arg.Any<IEnumerable<Triple>>(), [], ct))
            .Do(x => Assert.That(x.ArgAt<IEnumerable<Triple>>(1).Count(), Is.EqualTo(113)));

        await sut.AddDataSet(Target, ct);

        await storageProvider.Received(1)
            .UpdateGraphAsync(Arg.Any<string>(), Arg.Any<IEnumerable<Triple>>(), [], ct);
    }

    [Test]
    public async Task AddCollection_ShouldUploadAndDelayEachChunk()
    {
        var collection = new Collection { Code = "A" };

        await graphService.AddCollection(Target, collection, ct);

        await storageProvider.Received(1).UpdateGraphAsync(
            Uris.TermdatGraph,
            Arg.Any<IEnumerable<Triple>>(),
            Arg.Any<IEnumerable<Triple>>(),
            ct);
        await chunkDelay.Received(1).Wait(ct);
    }
}
