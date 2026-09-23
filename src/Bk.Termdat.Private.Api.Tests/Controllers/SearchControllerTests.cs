using System;
using System.Threading.Tasks;
using AutoMapper;
using Bk.Termdat.Business.Resource.Models;
using Bk.Termdat.Business.Services.Interfaces;
using Bk.Termdat.Private.Api.Controllers;
using Bogus;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;

namespace Bk.Termdat.Private.Api.Tests.Controllers;

[TestFixture]
internal class SearchControllerTests
{
    private SearchController searchController;

    private readonly ILogger<SearchController> loggerMock = Substitute.For<ILogger<SearchController>>();
    private readonly ISearchQueryService searchQueryServiceMock = Substitute.For<ISearchQueryService>();
    private readonly IDuplicateCheckService duplicateCheckServiceMock = Substitute.For<IDuplicateCheckService>();
    private readonly Faker<RecommendQuery> recommendQueryFaker = new();
    private SearchQuery searchQuery;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        searchQuery = new Faker<SearchQuery>().Generate();

        var searchPageFaker = new Faker<SearchPage>();
        searchQueryServiceMock.Search(searchQuery)
            .Returns(searchPageFaker.Generate());

        searchController = new SearchController(searchQueryServiceMock, duplicateCheckServiceMock, loggerMock);
    }

    [Test]
    public async Task Search_ShouldCallSearchService()
    {
        var result = await searchController.Search(searchQuery);

        await searchQueryServiceMock.Received().Search(searchQuery);
        Assert.That(result, Is.Not.Null);

        var response = result as OkObjectResult;
        Assert.That(response, Is.Not.Null);
        Assert.That(response.Value, Is.Not.Null);
        Assert.That(response.StatusCode, Is.EqualTo(200));
    }

    [Test]
    public async Task Recommend_ShouldCallSearchService()
    {
        var recommendQuery = recommendQueryFaker.Generate();
        var result = await searchController.Recommend(recommendQuery);

        await searchQueryServiceMock.Received().Recommend(recommendQuery.Phrase, recommendQuery.SourceLanguageIds);
        Assert.That(result, Is.Not.Null);

        var response = result as OkObjectResult;
        Assert.That(response, Is.Not.Null);
        Assert.That(response.Value, Is.Not.Null);
        Assert.That(response.StatusCode, Is.EqualTo(200));
    }

    [Test]
    public async Task Recommend_CatchesAllExceptionsAndReturnsEmpty()
    {
        var recommendQuery = recommendQueryFaker.Generate();

        searchQueryServiceMock
            .Recommend(recommendQuery.Phrase, recommendQuery.SourceLanguageIds)
            .Throws(new Exception("boom"));

        var result = await searchController.Recommend(recommendQuery);

        Assert.That(result, Is.Not.Null);

        var response = result as OkObjectResult;
        Assert.That(response, Is.Not.Null);
        Assert.That(response.Value, Is.Not.Null);
        Assert.That(response.StatusCode, Is.EqualTo(200));
    }

    [Test]
    public async Task Recommend_ShouldReturnBadRequestWhenRecommendQueryIsNull()
    {
        var result = await searchController.Recommend(null);

        Assert.That(result, Is.InstanceOf<BadRequestResult>());
    }

    [Test]
    public async Task Search_ShouldReturnBadRequestWhenSearchQueryIsNull()
    {
        var result = await searchController.Search(null);

        Assert.That(result, Is.InstanceOf<BadRequestResult>());
    }

    [Test]
    public async Task Search_WhenCalledWithInvalidEntryIds_ShouldReturnBadRequest()
    {
        searchQuery.EntryIds = "This is an unsplittable ID";

        searchQueryServiceMock
            .Search(searchQuery)
            .Throws(new AutoMapperMappingException("boom"));

        var result = await searchController.Search(searchQuery);

        Assert.That(result, Is.InstanceOf<BadRequestResult>());
    }
}
