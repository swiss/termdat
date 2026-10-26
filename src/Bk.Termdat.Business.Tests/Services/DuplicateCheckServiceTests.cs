using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Bk.Termdat.Business.Repositories;
using Bk.Termdat.Business.Resource.Models;
using Bk.Termdat.Business.Services;
using Bk.Termdat.Business.Services.Interfaces;
using NSubstitute;
using NSubstitute.ClearExtensions;
using NUnit.Framework;

namespace Bk.Termdat.Business.Tests.Services;

[TestFixture]
internal class DuplicateCheckServiceTests
{
    private readonly IDuplicateCheckRepository duplicateCheckRepositoryMock = Substitute.For<IDuplicateCheckRepository>();
    private readonly IExcelDataService excelDataServiceMock = Substitute.For<IExcelDataService>();

    private DuplicateCheckService service;

    [SetUp]
    public void SetUp()
    {
        duplicateCheckRepositoryMock.ClearSubstitute();
        excelDataServiceMock.ClearSubstitute();

        service = new DuplicateCheckService(duplicateCheckRepositoryMock, excelDataServiceMock);
    }

    [Test]
    public async Task GetDuplicateEntryIds_WhenCollectionIdProvided_ReturnsDuplicateIds()
    {
        var request = new DuplicateCheckRequest
        {
            CollectionId = 10,
            LanguageId = 20
        };

        duplicateCheckRepositoryMock.GetByCollectionIdAndLanguageId(10, 20)
            .Returns(
            [
                new DuplicateCheckMatch
            {
                EntryId = 1,
                MatchedEntryIds = [2, 3]
            }
            ]);

        var result = await service.GetDuplicateEntryIds(request);

        await duplicateCheckRepositoryMock.Received(1).GetByCollectionIdAndLanguageId(10, 20);
        Assert.That(result.Ids, Is.EquivalentTo([1, 2, 3]));
    }

    [Test]
    public async Task GetDuplicateEntryIds_WhenGlossaryIdProvided_ReturnsDuplicateIds()
    {
        var request = new DuplicateCheckRequest
        {
            GlossaryId = 100,
            LanguageId = 20
        };

        duplicateCheckRepositoryMock.GetByGlossaryIdAndLanguageId(100, 20)
            .Returns(
            [
                new DuplicateCheckMatch
            {
                EntryId = 5,
                MatchedEntryIds = [6]
            }
            ]);

        var result = await service.GetDuplicateEntryIds(request);

        await duplicateCheckRepositoryMock.Received(1).GetByGlossaryIdAndLanguageId(100, 20);
        Assert.That(result.Ids, Is.EquivalentTo([5, 6]));
    }

    [Test]
    public async Task GetDuplicateEntryIds_ShouldRemoveReverseMatches()
    {
        var request = new DuplicateCheckRequest
        {
            CollectionId = 10,
            LanguageId = 20
        };

        duplicateCheckRepositoryMock.GetByCollectionIdAndLanguageId(10, 20)
            .Returns(
            [
                new DuplicateCheckMatch
            {
                EntryId = 1,
                MatchedEntryIds = [2]
            },
            new DuplicateCheckMatch
            {
                EntryId = 2,
                MatchedEntryIds = [1, 3]
            }
            ]);

        var result = await service.GetDuplicateEntryIds(request);

        Assert.That(result.Ids, Is.EquivalentTo([1, 2, 2, 3]));
    }

    [Test]
    public async Task GetDuplicateEntryIds_WhenNoMatches_ReturnsEmptyIds()
    {
        var request = new DuplicateCheckRequest
        {
            CollectionId = 10,
            LanguageId = 20
        };

        duplicateCheckRepositoryMock.GetByCollectionIdAndLanguageId(10, 20)
            .Returns([]);

        var result = await service.GetDuplicateEntryIds(request);

        Assert.That(result.Ids, Is.Empty);
    }

    [Test]
    public async Task ExportDuplicateCheckXlsx_ShouldCreateExcelAndReturnResult()
    {
        var request = new DuplicateCheckRequest
        {
            CollectionId = 1,
            LanguageId = 2
        };

        var matches = new List<DuplicateCheckMatch>
    {
        new()
        {
            EntryId = 1,
            MatchedKeys = ["Text"],
            MatchedEntryIds = [2]
        }
    };

        var stream = new MemoryStream();

        duplicateCheckRepositoryMock
            .GetByCollectionIdAndLanguageId(request.CollectionId.Value, request.LanguageId)
            .Returns(matches);

        excelDataServiceMock
            .CreateDuplicateCheckExport(Arg.Any<IList<DuplicateCheckMatch>>())
            .Returns(("duplicate-check.xlsx", stream));

        var (fileName, content) = await service.ExportDuplicateCheckXlsx(request);

        Assert.That(fileName, Is.EqualTo("duplicate-check.xlsx"));
        Assert.That(content, Is.SameAs(stream));

        await excelDataServiceMock.Received(1)
            .CreateDuplicateCheckExport(Arg.Any<IList<DuplicateCheckMatch>>());
    }

    [Test]
    public async Task ExportDuplicateCheckXlsx_ShouldReturnEmptyResultWhenNoMatches()
    {
        var request = new DuplicateCheckRequest
        {
            CollectionId = 1,
            LanguageId = 2
        };

        duplicateCheckRepositoryMock
            .GetByCollectionIdAndLanguageId(request.CollectionId.Value, request.LanguageId)
            .Returns(new List<DuplicateCheckMatch>());

        var (fileName, content) = await service.ExportDuplicateCheckXlsx(request);

        Assert.That(fileName, Is.Null);
        Assert.That(content, Is.Null);

        await excelDataServiceMock.DidNotReceive()
            .CreateDuplicateCheckExport(Arg.Any<IList<DuplicateCheckMatch>>());
    }
}
