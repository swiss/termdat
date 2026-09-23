using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Bk.DocumentService.Client;
using Bk.Termdat.Business.Domain.Models;
using Bk.Termdat.Business.Repositories;
using Bk.Termdat.Business.Resource.Models;
using Bk.Termdat.Business.Resource.Models.Export;
using Bk.Termdat.Business.Services;
using Bk.Termdat.Business.Services.Interfaces;
using Bk.Termdat.CrossCutting.Exceptions;
using Bogus;
using NSubstitute;
using NSubstitute.ClearExtensions;
using NUnit.Framework;

namespace Bk.Termdat.Business.Tests.Services;

[TestFixture]
internal class WatchlistServiceTests
{
    private IWatchlistService watchlistService;
    private IEntryRepository entryRepositoryMock;
    private IAuthorizationService authorizationServiceMock;
    private IMapper mapperMock;
    private IEnumerable<Entry> entries;
    private readonly IDocumentService documentService = Substitute.For<IDocumentService>();

    [SetUp]
    public void SetUp()
    {
        var faker = new Faker<Entry>();
        entries = Enumerable.Range(1, 4)
            .Select(i =>
            {
                var entry = faker.Generate();
                entry.Id = i;
                return entry;
            });

        mapperMock = Substitute.For<IMapper>();
        entryRepositoryMock = Substitute.For<IEntryRepository>();
        authorizationServiceMock = Substitute.For<IAuthorizationService>();
        authorizationServiceMock.CanEdit(Arg.Any<Entry>()).Returns(true);
        mapperMock.Map<IEnumerable<WatchlistEntry>>(Arg.Any<object>())
            .Returns(callInfo =>
            {
                var source = callInfo.ArgAt<object>(0);
                if (source is IEnumerable<object> enumerable)
                {
                    var mapped = enumerable
                        .Select(_ => new WatchlistEntry())
                        .ToList();
                    return mapped;
                }

                return new List<WatchlistEntry>();
            });

        watchlistService = new WatchlistService(entryRepositoryMock, authorizationServiceMock, mapperMock, documentService);
    }

    [TearDown]
    public void TearDown()
    {
        documentService.ClearSubstitute();
        mapperMock.ClearSubstitute();
        entryRepositoryMock.ClearSubstitute();
        entryRepositoryMock.ClearSubstitute();
        authorizationServiceMock.ClearSubstitute();
    }

    [Test]
    public async Task GetEntriesShouldReturnEmptyArrayIfNonIsFound()
    {
        entryRepositoryMock.GetByIdsForWatchlist(Arg.Any<IEnumerable<int>>()).Returns(new List<Entry>());
        var result = (await watchlistService.GetEntries(Array.Empty<int>())).ToList();

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.Empty);

        await entryRepositoryMock.Received(1).GetByIdsForWatchlist(Arg.Is<IEnumerable<int>>(Array.Empty<int>()));
        mapperMock.Received(0).Map<IEnumerable<WatchlistEntry>>(Arg.Any<object>());
    }

    [Test]
    public async Task GetEntries_WithAccess_ShouldReturnMappedWatchlistEntries()
    {
        authorizationServiceMock
            .CanAddToWatchList(Arg.Any<Entry>())
            .Returns(callInfo =>
            {
                var entry = callInfo.ArgAt<Entry>(0);
                return entry.Id is 1 or 2 or 3;
            });
        entryRepositoryMock.GetByIdsForWatchlist(Arg.Any<IEnumerable<int>>()).Returns(entries);
        var result = (await watchlistService.GetEntries(new[] { 1, 2, 3 })).ToList();

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.Not.Empty);
        Assert.That(result, Has.Count.EqualTo(3));

        await entryRepositoryMock.Received(1).GetByIdsForWatchlist(Arg.Is<IEnumerable<int>>(entryIds => entryIds.All(e => e == 1 || e == 2 || e == 3)));
        mapperMock.Received(1).Map<IEnumerable<WatchlistEntry>>(Arg.Any<object>());
    }

    [Test]
    public async Task GetEntries_WithPartialAccess_ShouldReturnMappedAuthorizedWatchlistEntries()
    {
        authorizationServiceMock
            .CanAddToWatchList(Arg.Any<Entry>())
            .Returns(callInfo =>
            {
                var entry = callInfo.ArgAt<Entry>(0);
                return entry.Id is 1 or 2;
            });
        entryRepositoryMock.GetByIdsForWatchlist(Arg.Any<IEnumerable<int>>()).Returns(entries);
        var result = (await watchlistService.GetEntries(new[] { 1, 2, 3 })).ToList();

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.Not.Empty);
        Assert.That(result, Has.Count.EqualTo(2));

        await entryRepositoryMock.Received(1).GetByIdsForWatchlist(Arg.Is<IEnumerable<int>>(entryIds => entryIds.All(e => e == 1 || e == 2 || e == 3)));
        mapperMock.Received(1).Map<IEnumerable<WatchlistEntry>>(Arg.Any<object>());
    }

    [Test]
    public async Task CreateExportPdfShouldThrowIfNoEntriesFound()
    {
        entryRepositoryMock.GetByIdsForExport(Arg.Any<IEnumerable<int>>()).Returns(new List<Entry>());

        Assert.ThrowsAsync<EntityNotFoundException>(async () => await watchlistService.CreateExportPdf(new[] { 1, 2, 3 }));
        await entryRepositoryMock.Received(1).GetByIdsForExport(Arg.Is<IEnumerable<int>>(entryIds => entryIds.All(e => e == 1 || e == 2 || e == 3)));
        await documentService.DidNotReceiveWithAnyArgs().CreatePdfFromTemplate(Arg.Any<string>(), Arg.Any<object>(), Arg.Any<string>());
    }

    [Test]
    public async Task CreateExportPdfShouldReturn()
    {
        entryRepositoryMock.GetByIdsForExport(Arg.Any<IEnumerable<int>>()).Returns(entries);
        mapperMock.Map<List<WatchlistExportEntry>>(Arg.Any<object>()).Returns(new List<WatchlistExportEntry> { new() });

        Assert.DoesNotThrowAsync(async () => await watchlistService.CreateExportPdf(new[] { 1, 2, 3 }));
        await entryRepositoryMock.Received(1).GetByIdsForExport(Arg.Is<IEnumerable<int>>(entryIds => entryIds.All(e => e == 1 || e == 2 || e == 3)));
        mapperMock.Received(1).Map<List<WatchlistExportEntry>>(Arg.Any<object>());
        await documentService.ReceivedWithAnyArgs(1).CreatePdfFromTemplate(Arg.Any<string>(), Arg.Any<object>(), Arg.Any<string>());
    }
}
