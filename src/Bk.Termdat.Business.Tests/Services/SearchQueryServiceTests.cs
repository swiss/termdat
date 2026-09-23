using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Bk.Termdat.Business.Clients;
using Bk.Termdat.Business.Domain.Models;
using Bk.Termdat.Business.Repositories;
using Bk.Termdat.Business.Resource.Models;
using Bk.Termdat.Business.Services;
using Bk.Termdat.Business.Services.Interfaces;
using Bk.Termdat.CrossCutting.Exceptions;
using NSubstitute;
using NSubstitute.ClearExtensions;
using NUnit.Framework;
using Status = Bk.Termdat.Business.Resource.Models.Status;

namespace Bk.Termdat.Business.Tests.Services;

[TestFixture]
internal class SearchQueryServiceTests
{
    private SearchQueryService searchService;
    private readonly ISearchApiClient searchApiClientMock = Substitute.For<ISearchApiClient>();
    private readonly IEntryRepository entryRepositoryMock = Substitute.For<IEntryRepository>();
    private readonly ISearchStatisticService searchStatisticServiceMock = Substitute.For<ISearchStatisticService>();
    private readonly IAuthorizationService authorizationServiceMock = Substitute.For<IAuthorizationService>();
    private readonly IEntryNotePatternService entryNotePatternServiceMock = Substitute.For<IEntryNotePatternService>();
    private readonly IMapper mapperMock = Substitute.For<IMapper>();
    private SearchResult searchResult;
    private SearchResult emptySearchResult;
    private SearchFilterParameter searchFilterParameter;

    [SetUp]
    public void SetUp()
    {
        searchApiClientMock.ClearSubstitute();
        entryRepositoryMock.ClearSubstitute();
        mapperMock.ClearSubstitute();
        authorizationServiceMock.ClearSubstitute();

        searchResult = new SearchResult
        {
            EntryIds = [21, 22, 23],
            TotalEntryCount = 3,
        };

        emptySearchResult = new SearchResult
        {
            EntryIds = [],
            TotalEntryCount = 0,
        };

        searchFilterParameter = new SearchFilterParameter
        {
            Phrase = "Search"
        };

        searchApiClientMock.Search(null, null).ReturnsForAnyArgs(searchResult);
        mapperMock.Map<SearchFilterParameter>(Arg.Any<SearchQuery>()).Returns(searchFilterParameter);

        searchService = new SearchQueryService(searchApiClientMock, entryRepositoryMock, mapperMock, searchStatisticServiceMock, authorizationServiceMock, entryNotePatternServiceMock);
    }

    [TestCase("test", null, null, 0, 10)]
    [TestCase("test", new[] { 6 }, null, 10, 10)]
    [TestCase("test", new[] { 6 }, new[] { 7 }, 10, 10)]
    public async Task Search_CallsRepositoryFunctions(string searchPhrase, int[] languageIds = null, int[] targetLanguageIds = null, int pageIndex = 0, int pageSize = 100)
    {
        var query = new SearchQuery { Phrase = searchPhrase, SourceLanguageIds = languageIds, TargetLanguageIds = targetLanguageIds, PageIndex = pageIndex, PageSize = pageSize };

        await searchService.Search(query);
        await entryRepositoryMock.Received().GetByIdsForSearchResults(searchResult.EntryIds);
    }

    [Test]
    public void Search_WhenCalledWithTooManyEntryIdsForWriter_ThrowsException()
    {
        var entryIds = GetSeparatedEntryNumbers();
        var entryIdsArray = entryIds.Split(",");
        var allIdsAsNumbers = Array.ConvertAll(entryIdsArray, int.Parse);

        searchFilterParameter.EntryIds = allIdsAsNumbers;

        mapperMock.Map<SearchFilterParameter>(Arg.Any<SearchQuery>()).Returns(searchFilterParameter);

        authorizationServiceMock.IsAdmin().Returns(false);
        authorizationServiceMock.IsSuperUser().Returns(false);
        authorizationServiceMock.IsWriter().Returns(true);

        var query = new SearchQuery { EntryIds = entryIds };

        Assert.ThrowsAsync<SearchException>(() => searchService.Search(query));
    }

    [Test]
    public async Task Search_WhenCalledWithALotOfEntryIdsForSuperUser_DoesNotThrow()
    {
        var entryIds = GetSeparatedEntryNumbers();
        var entryIdsArray = entryIds.Split(",");
        var allIdsAsNumbers = Array.ConvertAll(entryIdsArray, int.Parse);

        searchFilterParameter.EntryIds = allIdsAsNumbers;

        authorizationServiceMock.IsAdmin().Returns(false);
        authorizationServiceMock.IsSuperUser().Returns(true);
        authorizationServiceMock.IsWriter().Returns(false);

        var query = new SearchQuery { EntryIds = entryIds };

        await searchService.Search(query);
        await entryRepositoryMock.Received().GetByIdsForSearchResults(Arg.Any<int[]>());
    }

    [Test]
    public async Task Search_CallsRepositoryFunctionsAsAdmin()
    {
        authorizationServiceMock.IsAdmin().Returns(true);

        var query = GenerateSearchQuery();
        await searchService.Search(query);
        await entryRepositoryMock.Received().GetByIdsForSearchResultsAdminOrWriterOrSuperReader(searchResult.EntryIds);
    }

    [Test]
    public async Task Search_CallsRepositoryFunctionsAsWriter()
    {
        authorizationServiceMock.IsWriter().Returns(true);

        var query = GenerateSearchQuery();
        await searchService.Search(query);
        await entryRepositoryMock.Received().GetByIdsForSearchResultsAdminOrWriterOrSuperReader(searchResult.EntryIds);
    }

    [Test]
    public async Task Search_CallsRepositoryFunctionsAsSuperReader()
    {
        authorizationServiceMock.IsSuperReader().Returns(true);

        var query = GenerateSearchQuery();
        await searchService.Search(query);
        await entryRepositoryMock.Received().GetByIdsForSearchResultsAdminOrWriterOrSuperReader(searchResult.EntryIds);
    }

    [Test]
    public async Task Search_CallsRepositoryFunctionsAsUnauthenticatedWithUnauthenticatedSearchQuery()
    {
        authorizationServiceMock.IsAdmin().Returns(false);
        authorizationServiceMock.IsSuperUser().Returns(false);
        authorizationServiceMock.IsWriter().Returns(false);

        var query = GenerateSearchQueryForUnauthenticated();
        searchApiClientMock.Search(null, null).ReturnsForAnyArgs(searchResult);

        await searchService.Search(query);
        await entryRepositoryMock.Received().GetByIdsForSearchResults(searchResult.EntryIds);
    }

    [TestCase("test", null)]
    [TestCase("test", new[] { 6 })]
    public async Task Recommend_CallsRepositoryFunctions(string searchPhrase, int[] languageIds = null)
    {
        await searchService.Recommend(searchPhrase, languageIds);
        await searchApiClientMock.Received().Recommend(searchPhrase, languageIds);
    }

    [Test]
    public async Task Search_WithoutHits_ShouldCallSearchStatisticService()
    {
        var query = new SearchQuery
        {
            Phrase = "test"
        };

        searchApiClientMock.Search(null, null).ReturnsForAnyArgs(emptySearchResult);

        await searchService.Search(query);

        await searchStatisticServiceMock.Received().LogUnsuccessfulSearch(searchFilterParameter, null);
    }

    [Test]
    public async Task Search_WithoutHitsAndWithoutPhrase_ShouldNotCallSearchStatisticService()
    {
        var query = new SearchQuery
        {
            Phrase = ""
        };

        searchApiClientMock.Search(null, null).ReturnsForAnyArgs(emptySearchResult);

        await searchService.Search(query);

        await searchStatisticServiceMock.Received(0).LogUnsuccessfulSearch(searchFilterParameter, null);
    }

    [Test]
    public async Task Search_WithValidData_DeterminesEntryNotePatterns()
    {
        authorizationServiceMock.IsAdmin().Returns(false);
        authorizationServiceMock.IsWriter().Returns(false);
        authorizationServiceMock.IsSuperUser().Returns(false);
        var query = new SearchQuery { Phrase = "A" };
        searchApiClientMock.Search(null, null).ReturnsForAnyArgs(searchResult);
        entryRepositoryMock.GetByIdsForSearchResults(searchResult.EntryIds).Returns(new List<Entry>
        {
            new()
            {
                Translations = new List<EntryLanguage>
                {
                    new() { Note = "X", Language = new Language { SortIndex = 1 }, Sequence = 1 }
                }
            }
        });

        await searchService.Search(query);

        entryNotePatternServiceMock.Received(1).DetermineEntryNotePatterns("X");
    }

    [Test]
    public async Task Search_AsNonAdmin_ResetsExcludeFilters()
    {
        authorizationServiceMock.IsAdmin().Returns(false);
        var query = new SearchQuery
        {
            AdminInfosExclude = true,
            CollectionsExclude = true,
            CreatedByExclude = true,
            GlossariesExclude = true,
            ModifiedByExclude = true,
            ModifiedPeriodExclude = true
        };

        await searchService.Search(query);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(query.AdminInfosExclude, Is.False);
            Assert.That(query.CollectionsExclude, Is.False);
            Assert.That(query.CreatedByExclude, Is.False);
            Assert.That(query.GlossariesExclude, Is.False);
            Assert.That(query.ModifiedByExclude, Is.False);
            Assert.That(query.ModifiedPeriodExclude, Is.False);
        }
    }
    private static SearchQuery GenerateSearchQueryForUnauthenticated()
    {
        return new SearchQuery
        {
            Phrase = "",
            PageIndex = 1,
            PageSize = 1,
            SourceLanguageIds = [2, 3],
            TargetLanguageIds = [3, 4, 5, 6, 7, 8],
            Offices = [1],
            LenochClassifications = [100],
            Collections = [10329],
            Status = [Status.Validated],
            Fields = new SearchQueryFields
            {
                Term = true,
                Name = true,
                Abbreviation = true,
                Phraseology = true,
                Definition = true,
                Note = true,
                Context = true,
                Source = true,
                Country = true,
            }
        };
    }

    private static SearchQuery GenerateSearchQuery()
    {
        return new SearchQuery
        {
            Phrase = "",
            PageIndex = 1,
            PageSize = 1,
            SourceLanguageIds = [2, 3],
            TargetLanguageIds = [3, 4, 5, 6, 7, 8],
            Offices = [1],
            LenochClassifications = [100],
            Collections = [10329],
            Status = [Status.Validated],
            AdminInfos = [2],
            CommentStatus = [CommentStatus.Empty],
            CreatedBy = "test@test.ch",
            ModifiedBy = "test@test.ch",
            ModifiedTo = DateTime.Today,
            ModifiedFrom = DateTime.Today,
            Fields = new SearchQueryFields
            {
                Term = true,
                Name = true,
                Abbreviation = true,
                Phraseology = true,
                Definition = true,
                Source = true,
                Country = true,
                Comment = true,
                Metadata = true
            }
        };
    }

    private static string GetSeparatedEntryNumbers()
    {
        return string.Join(',', Enumerable.Range(1, 300));
    }
}
