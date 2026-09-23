using Bk.Termdat.Business.Repositories;
using Bk.Termdat.LindasExport.Api.Interfaces;
using Bk.Termdat.LindasExport.Api.Models;
using Bk.Termdat.LindasExport.Api.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Bk.Termdat.LindasExport.Api.Tests.Services;

[TestFixture]
internal class LindasExportServiceTests
{
    private const string Target = "TestTarget";
    private readonly ILogger<LindasExportService> logger = NullLogger<LindasExportService>.Instance;
    private readonly ICollectionRepository collectionRepository = Substitute.For<ICollectionRepository>();
    private readonly ITermdatGraphService graphService = Substitute.For<ITermdatGraphService>();
    private readonly ILindasMapper lindasMapper = Substitute.For<ILindasMapper>();
    private ILindasExportService exportService = null!;
    private static readonly CancellationToken ct = CancellationToken.None;

    [SetUp]
    public void SetUp()
    {
        graphService.Targets.Returns(new[] { Target });
        exportService = new LindasExportService(logger, collectionRepository, graphService, lindasMapper);
    }

    [TearDown]
    public void TearDown()
    {
        lindasMapper.ClearSubstitute();
        graphService.ClearSubstitute();
        collectionRepository.ClearSubstitute();
    }

    [Test]
    public async Task ExportCollections_ShouldInitializeTarget()
    {
        collectionRepository.GetIdsForLindasExport().Returns([]);

        var result = await exportService.ExportCollections(ct);

        await graphService.Received(1).DeleteGraph(Target, ct);
        await graphService.Received(1).AddDataSet(Target, ct);
        Assert.That(result.SuccessfulTargets, Is.EqualTo(new[] { Target }));
        Assert.That(result.IsSuccess, Is.True);
    }

    [Test]
    public async Task ExportCollections_ForCollections_ShouldAddCollectionsToTarget()
    {
        var idsToExport = new[] { 1, 3, 3, 4 };
        collectionRepository.GetIdsForLindasExport().Returns(idsToExport);
        collectionRepository.GetByIdForLindasExport(Arg.Any<int>())
            .Returns(new Business.Domain.Models.Collection());
        lindasMapper.Map(Arg.Any<Business.Domain.Models.Collection>())
            .Returns(new Collection { Code = string.Empty });

        await exportService.ExportCollections(ct);

        await collectionRepository.Received(1).GetIdsForLindasExport();
        await collectionRepository.Received(idsToExport.Length).GetByIdForLindasExport(Arg.Any<int>());
        lindasMapper.Received(idsToExport.Length).Map(Arg.Any<Business.Domain.Models.Collection>());
        await graphService.Received(idsToExport.Length)
            .AddCollection(Target, Arg.Any<Collection>(), ct);
    }

    [Test]
    public async Task ExportCollections_WhenTargetFails_ShouldContinueWithNextTarget()
    {
        graphService.Targets.Returns(new[] { "Failed", "Healthy" });
        collectionRepository.GetIdsForLindasExport().Returns([]);
        graphService.DeleteGraph("Failed", ct).Returns(Task.FromException(new HttpRequestException()));

        var result = await exportService.ExportCollections(ct);

        await graphService.DidNotReceive().AddDataSet("Failed", ct);
        await graphService.Received(1).DeleteGraph("Healthy", ct);
        await graphService.Received(1).AddDataSet("Healthy", ct);
        Assert.That(result.FailedTargets, Is.EqualTo(new[] { "Failed" }));
        Assert.That(result.SuccessfulTargets, Is.EqualTo(new[] { "Healthy" }));
        Assert.That(result.IsSuccess, Is.False);
    }
}
