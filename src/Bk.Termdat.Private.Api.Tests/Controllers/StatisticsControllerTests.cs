using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bk.Termdat.Business.Resource.Models;
using Bk.Termdat.Business.Services.Interfaces;
using Bk.Termdat.Private.Api.Controllers;
using Bogus;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NUnit.Framework;

namespace Bk.Termdat.Private.Api.Tests.Controllers;

[TestFixture]
internal class StatisticsControllerTests
{
    private StatisticsController statisticController;

    private readonly IStatisticService statisticServiceMock = Substitute.For<IStatisticService>();
    private IEnumerable<StatisticSummary> statistics;
    private StatisticSearchQuery searchQuery;
    private StatisticSearchResult searchResult;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        var faker = new Faker<StatisticSummary>();

        searchQuery = new StatisticSearchQuery
        {
            Phrase = "DE,",
            FromDate = DateTime.UtcNow.AddDays(-365),
            ToDate = DateTime.UtcNow.AddDays(1)
        };

        statistics = Enumerable.Range(1, 8).Select(_ => faker.Generate());

        searchResult = new StatisticSearchResult
        {
            Statistics = statistics,
            TotalCount = 8
        };

        statisticServiceMock.GetBySearchQuery(searchQuery).ReturnsForAnyArgs(searchResult);
        statisticController = new StatisticsController(statisticServiceMock);
    }

    [Test]
    public async Task GetBySearchQuery_WhenCalled_ShouldCallStatisticService()
    {
        var result = await statisticController.GetBySearchQuery(searchQuery);

        await statisticServiceMock.Received().GetBySearchQuery(searchQuery);

        Assert.That(result, Is.Not.Null);
        var response = result as OkObjectResult;
        Assert.That(response, Is.Not.Null);
        Assert.That(response.Value, Is.Not.Null);
        Assert.That(response.StatusCode, Is.EqualTo(200));
    }

    [Test]
    public async Task Delete_WhenCalled_ShouldCallStatisticService()
    {
        var result = await statisticController.Delete(1);

        await statisticServiceMock.Received(1).Delete(Arg.Is(1));
        Assert.That(result is OkResult);
    }

    [Test]
    public async Task DeleteBySearchQuery_WhenCalled_ShouldCallStatisticService()
    {
        var result = await statisticController.Delete(searchQuery);

        await statisticServiceMock.Received().DeleteBySearchQuery(searchQuery);

        Assert.That(result, Is.Not.Null);
        Assert.That(result is OkResult);
    }
}
