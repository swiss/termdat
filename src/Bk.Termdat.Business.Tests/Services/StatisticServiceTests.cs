using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Bk.Termdat.Business.Domain;
using Bk.Termdat.Business.Domain.Models;
using Bk.Termdat.Business.Repositories;
using Bk.Termdat.Business.Resource.Models;
using Bk.Termdat.Business.Services;
using Bk.Termdat.CrossCutting.Exceptions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ClearExtensions;
using NUnit.Framework;

namespace Bk.Termdat.Business.Tests.Services;

[TestFixture]
internal class StatisticServiceTests
{
    private StatisticService statisticService;
    private IMapper mapperMock;
    private IStatisticRepository statisticRepositoryMock;
    private Statistic statistic;
    private StatisticSummary statisticSummary;
    private StatisticSummary newStatisticSummary;
    private StatisticCreate statisticCreate;
    private StatisticUpdate statisticUpdate;
    private StatisticDetail statisticDetail;
    private IEnumerable<Statistic> statistics;
    private IEnumerable<Statistic> veryOldStatistics;
    private IEnumerable<StatisticSummary> statisticSummaries;
    private StatisticSearchQuery searchQuery;
    private readonly IUnitOfWork unitOfWorkMock = Substitute.For<IUnitOfWork>();

    [SetUp]
    public void SetUp()
    {
        mapperMock = Substitute.For<IMapper>();
        statisticRepositoryMock = Substitute.For<IStatisticRepository>();

        statistic = new Statistic
        {
            Id = 1,
            SearchText = "MeinSuchtext",
            SearchFields = "AB, Na, Md, PH, VE",
            Languages = "DE, FR, IT",
            NumberOfSearches = 11,
            DateLastSearch = DateTime.UtcNow,
            Created = DateTime.UtcNow,
            CreatedBy = "pp",
            Modified = DateTime.UtcNow,
            ModifiedBy = "pp",
        };

        statisticSummary = new StatisticSummary
        {
            Id = 1,
            SearchText = "MeinSuchtext2",
            SearchFields = "AB, Na, Md, PH, VE",
            Languages = "DE, FR, IT",
            NumberOfSearches = 11,
            DateLastSearch = DateTime.UtcNow,
        };

        newStatisticSummary = new StatisticSummary
        {
            SearchText = "MeinSuchtext2",
            SearchFields = "AB, Na, Md, PH, VE",
            Languages = "DE, FR, IT",
            NumberOfSearches = 11,
            DateLastSearch = DateTime.UtcNow,
        };

        statisticCreate = new StatisticCreate
        {
            SearchText = "MeinSuchtext3",
            SearchFields = "AB, Na, Md",
            Languages = "DE, FR, IT, RM, EN",
            DateLastSearch = DateTime.UtcNow,
        };

        statisticUpdate = new StatisticUpdate
        {
            Id = 1,
            SearchText = "MeinSuchtext3",
            SearchFields = "AB, Na, Md",
            Languages = "DE, FR, IT, RM, EN",
            DateLastSearch = DateTime.UtcNow,
        };

        statisticDetail = new StatisticDetail
        {
            Id = 1,
            SearchText = "MeinSuchtext4",
            SearchFields = "AB, Na, Md, PH, VE",
            Languages = "DE, FR, IT, EN",
            NumberOfSearches = 11,
            DateLastSearch = DateTime.UtcNow,
            Created = DateTime.UtcNow,
            CreatedBy = "pp",
            Modified = DateTime.UtcNow,
            ModifiedBy = "pp",
        };

        statistics = new List<Statistic>
        {
            statistic,
            new()
            {
                Id = 2,
                SearchText = "MeinSuchtext3",
                SearchFields = "AB, Na, Md, PH, VE",
                Languages = "DE, FR, IT",
                NumberOfSearches = 11,
                DateLastSearch = DateTime.UtcNow,
                Created = DateTime.UtcNow,
                CreatedBy = "pp",
                Modified = DateTime.UtcNow,
                ModifiedBy = "pp",
            }
        };

        veryOldStatistics = new List<Statistic>
        {
            new()
            {
                Id = 9,
                SearchText = "MeinAlterSuchtext3",
                SearchFields = "AB, Na, Md, PH, VE",
                Languages = "DE, FR, IT",
                NumberOfSearches = 12,
                DateLastSearch = DateTime.UtcNow.AddDays(-700),
                Created = DateTime.UtcNow.AddDays(-700),
                CreatedBy = "pp Ahn",
                Modified = DateTime.UtcNow.AddDays(-700),
                ModifiedBy = "pp Ahn",
            },
            new()
            {
                Id = 10,
                SearchText = "MeinUralterSuchtext4",
                SearchFields = "AB, Na, Md, PH, VE",
                Languages = "DE, FR, IT",
                NumberOfSearches = 12,
                DateLastSearch = DateTime.UtcNow.AddDays(-1700),
                Created = DateTime.UtcNow.AddDays(-1700),
                CreatedBy = "pp Urahn",
                Modified = DateTime.UtcNow.AddDays(-1700),
                ModifiedBy = "pp Urahn",
            }
        };

        statisticSummaries = new List<StatisticSummary>
        {
            statisticSummary,
            new()
            {
                Id = 2,
                SearchText = "TermdatSuche",
                SearchFields = "AB, Na, Md, PH, VE",
                Languages = "DE, FR, IT, RM, EN",
                NumberOfSearches = 7,
                DateLastSearch = DateTime.UtcNow,
            }
        };

        searchQuery = new StatisticSearchQuery
        {
            Phrase = string.Empty,
            FromDate = DateTime.UtcNow.AddDays(-365),
            ToDate = DateTime.UtcNow,
        };

        statisticRepositoryMock.GetById(1).Returns(statistic);
        statisticRepositoryMock.GetAll().Returns(statistics);
        statisticRepositoryMock.GetBySummary(statisticSummary).Returns(statistic);
        statisticRepositoryMock.GetBySearchQuery(searchQuery).Returns(statistics);
        statisticRepositoryMock.GetObsoleteStatistics().Returns(veryOldStatistics);

        mapperMock.Map<Statistic>(statisticSummary).Returns(statistic);
        mapperMock.Map<StatisticDetail>(statistic).Returns(statisticDetail);
        mapperMock.Map<IEnumerable<StatisticSummary>>(statistics).Returns(statisticSummaries);
        mapperMock.Map<Statistic>(statisticUpdate).Returns(statistic);
        mapperMock.Map<Statistic>(statisticCreate).Returns(statistic);
        mapperMock.Map<StatisticUpdate>(statisticSummary).Returns(statisticUpdate);
        mapperMock.Map<StatisticCreate>(newStatisticSummary).Returns(statisticCreate);

        statisticService = new StatisticService(mapperMock, statisticRepositoryMock, unitOfWorkMock, Substitute.For<ILogger<StatisticService>>());
    }

    [TearDown]
    public void TearDown()
    {
        mapperMock.ClearSubstitute();
        statisticRepositoryMock.ClearSubstitute();
        unitOfWorkMock.ClearSubstitute();
    }

    [Test]
    public async Task GetAll_WhenCalled_ShouldReturnStatistic()
    {
        var allStatistics = await statisticService.GetAll();

        await statisticRepositoryMock.Received(1).GetAll();
        mapperMock.Received(1).Map<IEnumerable<StatisticSummary>>(statistics);
        Assert.That(allStatistics.Count, Is.EqualTo(2));
    }

    [Test]
    public async Task GetBySearchQuery_WhenCalled_ShouldReturnStatistic()
    {
        var searchResult = await statisticService.GetBySearchQuery(searchQuery);

        await statisticRepositoryMock.Received(1).GetBySearchQuery(searchQuery);
        mapperMock.Received(1).Map<IEnumerable<StatisticSummary>>(Arg.Any<IEnumerable<Statistic>>());

        Assert.That(searchResult.TotalCount, Is.EqualTo(2));
    }

    [Test]
    public async Task Create_WhenCalled_ShouldReturnStatisticDetail()
    {
        var result = await statisticService.Create(statisticCreate);

        mapperMock.Received().Map<Statistic>(statisticCreate);

        await statisticRepositoryMock.Received().Create(statistic);
        await unitOfWorkMock.Received().SaveChangesAsync();
        await statisticRepositoryMock.GetById(statistic.Id);
        mapperMock.Received().Map<StatisticDetail>(statistic);

        Assert.That(result, Is.EqualTo(statisticDetail));
    }

    [Test]
    public async Task CreateOrUpdateStatistic_WhenExisting_ShouldCallUpdate()
    {
        var result = await statisticService.CreateOrUpdateStatistic(statisticSummary);

        mapperMock.Received(1).Map<StatisticUpdate>(statisticSummary);

        Assert.That(result, Is.EqualTo(statisticDetail));
    }

    [Test]
    public async Task CreateOrUpdateStatistic_WhenNotExisting_ShouldCallCreate()
    {
        var result = await statisticService.CreateOrUpdateStatistic(newStatisticSummary);

        mapperMock.Received(1).Map<StatisticCreate>(newStatisticSummary);

        Assert.That(result, Is.EqualTo(statisticDetail));
    }

    [Test]
    public async Task Update_WhenNotExisting_ShouldThrowEntityNotFoundException()
    {
        statisticUpdate.Id = 999;
        statisticRepositoryMock.GetById(0).Returns(Task.FromResult<Statistic>(null));

        Assert.ThrowsAsync<EntityNotFoundException>(async () => await statisticService.Update(statisticUpdate));
        await statisticRepositoryMock.Received().GetById(999);
    }

    [Test]
    public async Task Update_WhenCalled_ShouldReturnStatisticDetail()
    {
        var firstStatistic = statistics.First();
        mapperMock.Map<Statistic>(statisticUpdate).Returns(firstStatistic);
        mapperMock.Map<StatisticDetail>(statistic).Returns(statisticDetail);
        statisticRepositoryMock.Update(statistic, firstStatistic);

        var result = await statisticService.Update(statisticUpdate);
        Assert.That(result, Is.EqualTo(statisticDetail));
    }

    [Test]
    public async Task Delete_WhenNotExisting_ShouldThrowEntityNotFoundException()
    {
        statisticUpdate.Id = 0;
        statisticRepositoryMock.GetById(0).Returns(Task.FromResult<Statistic>(null));

        Assert.ThrowsAsync<EntityNotFoundException>(async () => await statisticService.Delete(statisticUpdate.Id));
        await statisticRepositoryMock.Received().GetById(0);
    }

    [Test]
    public async Task Delete_WhenCalled_ShouldCallRepository()
    {
        await statisticService.Delete(1);

        await statisticRepositoryMock.Received(1).GetById(Arg.Is(1));
        statisticRepositoryMock.Received(1).Delete(Arg.Any<Statistic>());
        await unitOfWorkMock.Received(1).SaveChangesAsync();
    }

    [Test]
    public async Task DeleteBySearchQuery_WhenCalled_ShouldCallRepository()
    {
        await statisticService.DeleteBySearchQuery(searchQuery);

        await statisticRepositoryMock.Received(1).GetBySearchQuery(searchQuery);

        statisticRepositoryMock.Received(2).Delete(Arg.Any<Statistic>());
    }

    [Test]
    public async Task DeleteObsoleteStatistics_WhenCalled_ShouldCallRepository()
    {
        await statisticService.DeleteObsoleteStatistics();

        await statisticRepositoryMock.Received(1).GetObsoleteStatistics();

        statisticRepositoryMock.Received(1).DeleteStatistics(Arg.Any<IEnumerable<Statistic>>());
    }
}
