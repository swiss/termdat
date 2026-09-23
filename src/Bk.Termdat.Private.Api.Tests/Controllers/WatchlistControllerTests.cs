using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Bk.Termdat.Business.Resource.Models;
using Bk.Termdat.Business.Services.Interfaces;
using Bk.Termdat.Private.Api.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;

namespace Bk.Termdat.Private.Api.Tests.Controllers;

[TestFixture]
internal class WatchlistControllerTests
{
    private WatchlistController watchlistController;
    private List<WatchlistEntry> entries;

    private readonly ILogger<WatchlistController> loggerMock = Substitute.For<ILogger<WatchlistController>>();
    private readonly IWatchlistService watchlistServiceMock = Substitute.For<IWatchlistService>();
    private readonly IExcelDataService excelDataServiceMock = Substitute.For<IExcelDataService>();

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        entries = [new WatchlistEntry()];
        watchlistServiceMock.GetEntries(Arg.Any<IEnumerable<int>>()).ReturnsForAnyArgs(entries);
        watchlistServiceMock.CreateExportPdf(Arg.Any<IEnumerable<int>>());

        watchlistController = new WatchlistController(watchlistServiceMock, excelDataServiceMock, loggerMock);
    }

    [Test]
    public async Task GetEntries_WithNullQuery_ShouldReturnBadRequest()
    {
        var result = await watchlistController.GetEntries(null);

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<BadRequestResult>());
        var response = result as BadRequestResult;
        Assert.That(response, Is.Not.Null);
    }

    [Test]
    public async Task GetEntries_WithValidData_ShouldGetWatchlistEntries()
    {
        var result = await watchlistController.GetEntries(new WatchlistQuery { EntryIds = new[] { 1, 2, 3, 4 } });

        Assert.That(result, Is.Not.Null);
        var response = result as OkObjectResult;
        Assert.That(response, Is.Not.Null);
        Assert.That(response.Value, Is.Not.Null);
        Assert.That(response.Value, Is.EqualTo(entries));

        await watchlistServiceMock.Received(1).GetEntries(Arg.Is<IEnumerable<int>>(entryIds => entryIds.All(e => e == 1 || e == 2 || e == 3 || e == 4)));
    }

    [Test]
    public async Task ExportPdf_WithNullQuery_ShouldReturnBadRequestResult()
    {
        var result = await watchlistController.ExportPdf(null);

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<BadRequestResult>());
        var response = result as BadRequestResult;
        Assert.That(response, Is.Not.Null);

        await watchlistServiceMock.DidNotReceive().CreateExportPdf(Arg.Any<IEnumerable<int>>());
    }

    [Test]
    public async Task ExportPdf_WithoutEntryIds_ShouldReturnBadRequest()
    {
        var result = await watchlistController.ExportPdf(new WatchlistQuery { EntryIds = System.Array.Empty<int>() });

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<BadRequestResult>());
        var response = result as BadRequestResult;
        Assert.That(response, Is.Not.Null);

        await watchlistServiceMock.DidNotReceive().CreateExportPdf(Arg.Any<IEnumerable<int>>());
    }

    [Test]
    public async Task ExportPdf_WithValidData_ShouldTriggerExportAndReturnFileResult()
    {
        watchlistServiceMock.CreateExportPdf(Arg.Any<IEnumerable<int>>()).Returns(new MemoryStream());

        var result = await watchlistController.ExportPdf(new WatchlistQuery { EntryIds = [42, 666] });

        Assert.That(result, Is.Not.Null);
        var response = result as FileResult;
        Assert.That(response, Is.Not.Null);

        await watchlistServiceMock.Received(1)
            .CreateExportPdf(Arg.Is<IEnumerable<int>>(entryIds => entryIds.All(e => e == 42 || e == 666)));
    }

    [Test]
    public async Task ExportXlsx_WithNullQuery_ShouldReturnBadRequest()
    {
        var result = await watchlistController.ExportXlsx(null);

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<BadRequestResult>());
        await excelDataServiceMock.DidNotReceive().CreateExport(Arg.Any<IReadOnlyList<int>>());
    }

    [Test]
    public async Task ExportXlsx_WithEmptyEntryIds_ShouldReturnBadRequest()
    {
        var result = await watchlistController.ExportXlsx(new WatchlistQuery { EntryIds = [] });

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<BadRequestResult>());
        await excelDataServiceMock.DidNotReceive().CreateExport(Arg.Any<IReadOnlyList<int>>());
    }

    [Test]
    public async Task ExportXlsx_WithValidData_ShouldReturnFileWithCorrectProperties()
    {
        const string expectedFileName = "Export-20251111-111100.xlsx";
        var expectedStream = new MemoryStream();
        excelDataServiceMock.CreateExport(Arg.Any<IReadOnlyList<int>>()).Returns((expectedFileName, expectedStream));

        var result = await watchlistController.ExportXlsx(new WatchlistQuery { EntryIds = [42, 666] });

        Assert.That(result, Is.Not.Null);
        var fileResult = result as FileStreamResult;
        Assert.That(fileResult, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(fileResult.FileDownloadName, Is.EqualTo(expectedFileName));
            Assert.That(fileResult.ContentType, Is.EqualTo("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"));
            Assert.That(fileResult.FileStream, Is.EqualTo(expectedStream));
        }

        await excelDataServiceMock.Received(1).CreateExport(Arg.Is<IReadOnlyList<int>>(entryIds =>
            entryIds.Count == 2 && entryIds[0] == 42 && entryIds[1] == 666));
    }
}
