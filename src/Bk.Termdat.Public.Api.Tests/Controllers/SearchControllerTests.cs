using System.Threading.Tasks;
using AutoFixture;
using Bk.Termdat.Business.Public.Api.Resource.Models;
using Bk.Termdat.Business.Public.Api.Services;
using Bk.Termdat.Public.Api.Controllers;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ClearExtensions;
using NUnit.Framework;

namespace Bk.Termdat.Public.Api.Tests.Controllers;

[TestFixture]
public class SearchControllerTests
{
    private readonly ISearchService searchService = Substitute.For<ISearchService>();
    private readonly IAnalyticsService analyticsService = Substitute.For<IAnalyticsService>();
    private readonly Fixture fixture = new();

    private SearchController searchController = null!;

    [SetUp]
    public void SetUp()
    {
        searchController = new SearchController(searchService, analyticsService);
    }

    [TearDown]
    public void TearDown()
    {
        analyticsService.ClearSubstitute();
        searchService.ClearSubstitute();
    }

    [Test]
    public async Task SearchReturnsResults()
    {
        var searchParameter = fixture.Create<SearchParameter>();
        var entryDetails = fixture.CreateMany<EntryDetail>();
        searchService.Search(searchParameter).Returns(entryDetails);

        var result = await searchController.Search(searchParameter);

        await searchService.Received(1).Search(searchParameter);
        Assert.That(result, Is.Not.Null);
        var okResult = result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult.Value, Is.EqualTo(entryDetails));
    }

    [Test]
    public async Task Search_ShouldTrackEvent()
    {
        var searchParameter = new SearchParameter { SearchTerm = "test_search_term" };

        await searchController.Search(searchParameter);

        await analyticsService.Received(1).TrackEvent("Public Search", "Perform", Arg.Is(searchParameter.SearchTerm));
    }
}
