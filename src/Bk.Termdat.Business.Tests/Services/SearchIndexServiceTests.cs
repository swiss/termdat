using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bk.Termdat.Business.Domain.Projections;
using Bk.Termdat.Business.Repositories;
using Bk.Termdat.Business.Services;
using Bk.Termdat.CrossCutting.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using NSubstitute.ClearExtensions;
using NUnit.Framework;

namespace Bk.Termdat.Business.Tests.Services;

[TestFixture]
internal class SearchIndexServiceTests
{
    private SearchIndexService searchIndexService;
    private IOptions<SearchOptions> searchOptionsMock;
    private readonly IEntryRepository entryRepositoryMock = Substitute.For<IEntryRepository>();
    private readonly ISearchRepository searchRepositoryMock = Substitute.For<ISearchRepository>();
    private readonly SearchOptions searchOptions = new()
    {
        IndexPageSize = 15000,
    };
    private readonly SearchOptions searchOptionsMissingPageSize = new()
    {
        IndexPageSize = 0,
    };

    private const int defaultPageSize = 5000;

    [SetUp]
    public void SetUp()
    {
        searchOptionsMock = Substitute.For<IOptions<SearchOptions>>();
        searchOptionsMock.Value.Returns(searchOptions);

        searchIndexService = new SearchIndexService(entryRepositoryMock,
            searchRepositoryMock, Substitute.For<ILogger<SearchIndexService>>(), searchOptionsMock);
    }

    [TearDown]
    public void TearDown()
    {
        entryRepositoryMock.ClearSubstitute();
        searchRepositoryMock.ClearSubstitute();
    }

    [Test]
    public async Task CreateIndex_WithPageSizeParameter_CallsRepositoryFunctions()
    {
        var moreIndexEntries = new List<IndexEntry>
        {
            new() { EntryId = 41 },
            new() { EntryId = 42 },
            new() { EntryId = 43 }
        }.ToAsyncEnumerable();
        // ReSharper disable PossibleMultipleEnumeration
        entryRepositoryMock.GetPageForIndex(0, searchOptions.IndexPageSize).ReturnsForAnyArgs(moreIndexEntries);

        await searchIndexService.CreateIndex(CancellationToken.None);

#pragma warning disable 4014
        entryRepositoryMock.Received().GetPageForIndex(0, searchOptions.IndexPageSize);
#pragma warning restore 4014
        await searchRepositoryMock.Received().ImportEntries(moreIndexEntries, CancellationToken.None);
        // ReSharper restore PossibleMultipleEnumeration
    }

    [Test]
    public async Task CreateIndex_WithoutPageSizeParameter_CallsRepositoryFunctions()
    {
        searchOptionsMock.Value.Returns(searchOptionsMissingPageSize);

        var moreIndexEntries = new List<IndexEntry>
        {
            new() { EntryId = 41 },
            new() { EntryId = 42 },
            new() { EntryId = 43 }
        }.ToAsyncEnumerable();
        // ReSharper disable PossibleMultipleEnumeration
        entryRepositoryMock.GetPageForIndex(0, defaultPageSize).ReturnsForAnyArgs(moreIndexEntries);

        await searchIndexService.CreateIndex(CancellationToken.None);

#pragma warning disable 4014
        entryRepositoryMock.Received().GetPageForIndex(0, defaultPageSize);
#pragma warning restore 4014
        await searchRepositoryMock.Received().ImportEntries(moreIndexEntries, CancellationToken.None);
        // ReSharper restore PossibleMultipleEnumeration
    }
}
