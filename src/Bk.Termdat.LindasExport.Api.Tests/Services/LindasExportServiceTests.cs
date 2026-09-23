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
    private readonly ILogger<LindasExportService> logger = NullLogger<LindasExportService>.Instance;
    private readonly ICollectionRepository collectionRepository = Substitute.For<ICollectionRepository>();
    private readonly ITermdatGraphService termdatGraphService = Substitute.For<ITermdatGraphService>();
    private readonly ILindasMapper lindasMapper = Substitute.For<ILindasMapper>();
    private ILindasExportService lindasExportService = null!;
    private static readonly CancellationToken ct = CancellationToken.None;

    [SetUp]
    public void SetUp()
    {
        lindasExportService = new LindasExportService(logger, collectionRepository, termdatGraphService, lindasMapper);
    }

    [TearDown]
    public void TearDown()
    {
        lindasMapper.ClearSubstitute();
        termdatGraphService.ClearSubstitute();
        collectionRepository.ClearSubstitute();
    }

    [Test]
    public async Task ExportCollections_ShouldDeleteGraph()
    {
        collectionRepository.GetIdsForLindasExport().Returns([]);

        await lindasExportService.ExportCollections(CancellationToken.None);

        await termdatGraphService.Received(1).DeleteGraph(ct);
    }

    [Test]
    public async Task ExportCollections_ForCollections_ShouldAddCollectionsToGraph()
    {
        var idsToExport = new[] { 1, 3, 3, 4 };
        collectionRepository.GetIdsForLindasExport().Returns(idsToExport);
        collectionRepository.GetByIdForLindasExport(Arg.Any<int>()).Returns(new Business.Domain.Models.Collection());
        lindasMapper.Map(Arg.Any<Business.Domain.Models.Collection>()).Returns(new Collection { Code = string.Empty });

        await lindasExportService.ExportCollections(CancellationToken.None);

        await termdatGraphService.Received(1).DeleteGraph(ct);
        await termdatGraphService.Received(1).AddDataSet(ct);
        await collectionRepository.Received(1).GetIdsForLindasExport();
        await collectionRepository.Received(idsToExport.Length).GetByIdForLindasExport(Arg.Any<int>());
        lindasMapper.Received(idsToExport.Length).Map(Arg.Any<Business.Domain.Models.Collection>());
        await termdatGraphService.Received(idsToExport.Length).AddCollection(Arg.Any<Collection>(), ct);
    }
}
