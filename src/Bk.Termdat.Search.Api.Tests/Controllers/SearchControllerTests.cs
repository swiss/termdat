using System;
using System.Collections.Generic;
using AutoFixture;
using Bk.Termdat.Business.Repositories;
using Bk.Termdat.Business.Resource.Models;
using Bk.Termdat.Business.Services.Interfaces;
using Bk.Termdat.Search.Api.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ClearExtensions;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;

namespace Bk.Termdat.Search.Api.Tests.Controllers;

[TestFixture]
internal class SearchControllerTests
{
    private SearchController searchController;

    private readonly ILogger<SearchController> loggerMock = Substitute.For<ILogger<SearchController>>();
    private readonly ISearchRepository searchRepositoryMock = Substitute.For<ISearchRepository>();
    private readonly IAuthorizationService authorizationServiceMock = Substitute.For<IAuthorizationService>();
    private readonly Fixture fixture = new();

    private SearchFilterParameter searchFilterParameter;
    private SearchFields searchFields;
    private bool includeDeleted;
    private SearchResult searchResult;
    private string searchPhrase;
    private IEnumerable<int> languageIds;
    private IEnumerable<string> recommendResult;

    [SetUp]
    public void SetUp()
    {
        searchFilterParameter = fixture.Create<SearchFilterParameter>();
        searchFields = fixture.Create<SearchFields>();
        includeDeleted = fixture.Create<bool>();
        searchResult = fixture.Create<SearchResult>();
        searchPhrase = fixture.Create<string>();
        languageIds = fixture.CreateMany<int>();
        recommendResult = fixture.CreateMany<string>();

        authorizationServiceMock.IsAdmin().Returns(includeDeleted);
        searchRepositoryMock.Search(searchFilterParameter, searchFields, includeDeleted)
            .Returns(searchResult);
        searchRepositoryMock.Recommend(searchPhrase, languageIds, includeDeleted)
            .Returns(recommendResult);

        searchController = new SearchController(searchRepositoryMock, authorizationServiceMock, loggerMock);
    }

    [TearDown]
    public void TearDown()
    {
        searchRepositoryMock.ClearSubstitute();
        authorizationServiceMock.ClearSubstitute();
    }

    [Test]
    public void SearchCallsSearchRepository()
    {
        var result = searchController.Search(searchFilterParameter, searchFields);

        searchRepositoryMock.Received().Search(searchFilterParameter, searchFields, includeDeleted);
        Assert.That(result, Is.Not.Null);

        var response = result as OkObjectResult;
        Assert.That(response, Is.Not.Null);
        Assert.That(response.Value, Is.Not.Null);
        Assert.That(response.StatusCode, Is.EqualTo(200));
    }

    [TestCase(true, true)]
    [TestCase(false, false)]
    public void SearchWithIsAdminCallsSearchRepository(bool expected, bool isAdmin)
    {
        authorizationServiceMock.IsAdmin().Returns(isAdmin);
        searchRepositoryMock.Search(searchFilterParameter, searchFields, isAdmin, isAdmin)
            .Returns(searchResult);

        var result = searchController.Search(searchFilterParameter, searchFields);

        searchRepositoryMock.Received().Search(searchFilterParameter, searchFields, expected, expected);
        Assert.That(result, Is.Not.Null);

        var response = result as OkObjectResult;
        Assert.That(response, Is.Not.Null);
        Assert.That(response.Value, Is.Not.Null);
        Assert.That(response.StatusCode, Is.EqualTo(200));
    }

    [Test]
    public void RecommendCallsSearchRepository()
    {
        var result = searchController.Recommend(searchPhrase, languageIds);

        searchRepositoryMock.Received().Recommend(searchPhrase, languageIds, includeDeleted, includeDeleted);
        Assert.That(result, Is.Not.Null);

        var response = result as OkObjectResult;
        Assert.That(response, Is.Not.Null);
        Assert.That(response.Value, Is.Not.Null);
        Assert.That(response.StatusCode, Is.EqualTo(200));
    }

    [TestCase(true, true)]
    [TestCase(false, false)]
    public void RecommendWithAuthorizationCallsSearchRepository(bool expected, bool isAdmin)
    {
        authorizationServiceMock.IsAdmin().Returns(isAdmin);
        searchRepositoryMock.Recommend(searchPhrase, languageIds, isAdmin, isAdmin)
            .Returns(recommendResult);

        var result = searchController.Recommend(searchPhrase, languageIds);

        searchRepositoryMock.Received().Recommend(searchPhrase, languageIds, expected, expected);
        Assert.That(result, Is.Not.Null);

        var response = result as OkObjectResult;
        Assert.That(response, Is.Not.Null);
        Assert.That(response.Value, Is.Not.Null);
        Assert.That(response.StatusCode, Is.EqualTo(200));
    }

    [Test]
    public void RecommendCatchesAllExceptionsAndReturnsEmpty()
    {

        searchRepositoryMock
            .Recommend(searchPhrase, languageIds, includeDeleted)
            .Throws(new Exception("boom"));

        var result = searchController.Recommend(searchPhrase, languageIds);

        Assert.That(result, Is.Not.Null);

        var response = result as OkObjectResult;
        Assert.That(response, Is.Not.Null);
        Assert.That(response.Value, Is.Not.Null);
        Assert.That(response.StatusCode, Is.EqualTo(200));
    }

    [Test]
    public void RecommendShouldReturnBadRequestWhenRecommendQueryIsNull()
    {
        var result = searchController.Recommend(null);

        Assert.That(result, Is.InstanceOf<BadRequestResult>());
    }

    [Test]
    public void SearchShouldReturnBadRequestWhenSearchQueryIsNull()
    {
        var result = searchController.Search(null, null);

        Assert.That(result, Is.InstanceOf<BadRequestResult>());
    }
}
