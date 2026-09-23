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
    [SetUp]
    public void SetUp()
    {
        _termdatGraphOptions.Value.Returns(new TermdatGraphOptions { ChunkSize = 1_000 });
        _lindasExportOptions.Value.Returns(new LindasExportOptions { AddCubeLinkData = false });

        _sparqlClientFactory.GetStorageProviders().Returns(new Dictionary<string, IAsyncStorageProvider>
        {
            ["TestTarget"] = _asyncStorageProvider
        });

        _graphService = new TermdatGraphService(_logger, _lindasExportOptions, _termdatGraphOptions, _sparqlClientFactory);
    }

    [TearDown]
    public void TearDown()
    {
        _sparqlClientFactory.ClearSubstitute();
        _asyncStorageProvider.ClearSubstitute();
    }

    [OneTimeTearDown]
    public void Cleanup()
    {
        _asyncStorageProvider.Dispose();
    }

    private readonly ILogger<TermdatGraphService> _logger = NullLogger<TermdatGraphService>.Instance;
    private readonly IAsyncStorageProvider _asyncStorageProvider = Substitute.For<IAsyncStorageProvider>();
    private readonly IOptions<TermdatGraphOptions> _termdatGraphOptions = Substitute.For<IOptions<TermdatGraphOptions>>();
    private readonly IOptions<LindasExportOptions> _lindasExportOptions = Substitute.For<IOptions<LindasExportOptions>>();
    private readonly ISparqlClientFactory _sparqlClientFactory = Substitute.For<ISparqlClientFactory>();
    private ITermdatGraphService _graphService = null!;
    private static readonly CancellationToken ct = CancellationToken.None;

    [Test]
    public void DeleteGraph_ShouldCallDeleteGraphOnStorageProvider()
    {
        _graphService.DeleteGraph(ct);

        _asyncStorageProvider.Received(1).DeleteGraphAsync(Uris.TermdatGraph, ct);
    }

    [Test]
    public void AddCollection_ForNullCollection_ShouldThrowArgumentNullException()
    {
        Assert.That(() => _graphService.AddCollection(null!, ct), Throws.InstanceOf<ArgumentNullException>());
    }

    [Test]
    public void AddDataSet_ShouldCallUpdateGraphOnStorageProvider()
    {
        _asyncStorageProvider.When(x => x.UpdateGraphAsync(Arg.Any<string>(), Arg.Any<IEnumerable<Triple>>(), [], ct))
            .Do(x => Assert.That(x.ArgAt<IEnumerable<Triple>>(1).Count(), Is.EqualTo(26)));

        _graphService.AddDataSet(ct);

        _asyncStorageProvider.Received(1)
            .UpdateGraphAsync(Arg.Any<string>(), Arg.Any<IEnumerable<Triple>>(), [], ct);
    }

    [Test]
    public async Task AddDataSet_WithEnabledCubeLinkData_ShouldAddAdditionalData()
    {
        _lindasExportOptions.Value.Returns(new LindasExportOptions { AddCubeLinkData = true });
        var sut = new TermdatGraphService(_logger, _lindasExportOptions, _termdatGraphOptions, _sparqlClientFactory);
        _asyncStorageProvider.When(x => x.UpdateGraphAsync(Arg.Any<string>(), Arg.Any<IEnumerable<Triple>>(), [], ct))
            .Do(x => Assert.That(x.ArgAt<IEnumerable<Triple>>(1).Count(), Is.EqualTo(113)));

        await sut.AddDataSet(ct);

        await _asyncStorageProvider.Received(1)
            .UpdateGraphAsync(Arg.Any<string>(), Arg.Any<IEnumerable<Triple>>(), [], ct);
    }

    [Test]
    public void AddCollection_ForValidCollection_ShouldCallUpdateGraphOnStorageProvider()
    {
        var collection = new Collection { Code = "A" };

        _graphService.AddCollection(collection, ct);

        _asyncStorageProvider.Received(1).UpdateGraphAsync(Arg.Is(Uris.TermdatGraph), Arg.Any<IEnumerable<Triple>>(),
            Arg.Any<IEnumerable<Triple>>(), ct);
    }
}
