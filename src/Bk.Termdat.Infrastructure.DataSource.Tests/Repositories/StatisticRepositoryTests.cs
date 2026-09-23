using System;
using System.Linq;
using System.Threading.Tasks;
using Bk.Termdat.Business.Domain.Models;
using Bk.Termdat.Business.Resource.Models;
using Bk.Termdat.Infrastructure.DataSource.Repositories;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Bk.Termdat.Infrastructure.DataSource.Tests.Repositories;

[TestFixture]
internal class StatisticRepositoryTests
{
    private StatisticRepository statisticRepository;
    private DataContext context;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        context = DatabaseHelper.PrepareContext(Guid.NewGuid().ToString("N"));
        statisticRepository = new StatisticRepository(context);
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        context.Dispose();
    }

    [TestCase]
    public async Task GetById_WhenCalled_ShouldReturnStatistic()
    {
        var data = await statisticRepository.GetById(2);

        Assert.That(data, Is.Not.Null);
        Assert.That(data.SearchText.Equals("Gargamel"));
        Assert.That(data.CreatedBy.Equals("test@test.ch"));
    }

    [TestCase]
    public async Task GetAll_WhenCalled_ShouldReturnDataWithMinimumFiveSearches()
    {
        var data = (await statisticRepository.GetAll()).ToList();
        var statistic = data.First();

        Assert.That(data, Is.Not.Null);
        Assert.That(statistic.SearchText, Is.Not.Empty);

        statistic = data.Last();

        Assert.That(data, Is.Not.Null);
        Assert.That(statistic.SearchText, Is.Not.Empty);
    }

    [Test]
    public async Task Create_WhenCalled_ShouldAddStatistic()
    {
        var builder = new DbContextOptionsBuilder<DataContext>()
           .UseInMemoryDatabase(Guid.NewGuid().ToString("N"));

        var createContext = new DataContext(builder.Options);
        var repository = new StatisticRepository(createContext);

        var newStatistic = new Statistic
        {
            SearchText = "Gargolye",
            SearchFields = "AB, Na, Md, PH, VE",
            Languages = "DE, FR",
            NumberOfSearches = 1,
            Created = DateTime.UtcNow,
            CreatedBy = "me@me.com",
            Modified = DateTime.UtcNow,
            ModifiedBy = "me@update.com",
        };

        await repository.Create(newStatistic);

        await createContext.SaveChangesAsync();

        Assert.That(newStatistic.Id, Is.Not.Default);
    }

    [Test]
    public async Task Update_WhenCalled_ShouldUpdateStatistic()
    {
        var databaseStatistic = await statisticRepository.GetById(5);
        var updatedStatistic = new Statistic
        {
            Id = databaseStatistic.Id,
            SearchText = "Friedenspanzer2",
            SearchFields = "AB, Na",
            Languages = "DE, FR",
            NumberOfSearches = 19,
            Modified = DateTime.UtcNow,
            ModifiedBy = "me@me.kong",
        };

        statisticRepository.Update(databaseStatistic, updatedStatistic);

        await context.SaveChangesAsync();

        var result = await statisticRepository.GetById(5);
        Assert.That(result.SearchText, Is.EqualTo(updatedStatistic.SearchText));
        Assert.That(result.SearchFields, Is.EqualTo(updatedStatistic.SearchFields));
        Assert.That(result.Languages, Is.EqualTo(updatedStatistic.Languages));
        Assert.That(result.NumberOfSearches, Is.EqualTo(updatedStatistic.NumberOfSearches));
        Assert.That(result.Created, Is.EqualTo(updatedStatistic.Created));
        Assert.That(result.CreatedBy, Is.EqualTo(updatedStatistic.CreatedBy));
        Assert.That(result.Id, Is.EqualTo(updatedStatistic.Id));
        Assert.That(result.Modified, Is.EqualTo(updatedStatistic.Modified));
        Assert.That(result.ModifiedBy, Is.EqualTo(updatedStatistic.ModifiedBy));
    }

    [Test]
    public async Task Delete_WhenCalled_ShouldRemoveStatisic()
    {
        var newStatistic = new Statistic
        {
            Id = 99,
            SearchText = "UralteSuche",
            SearchFields = "AB, Na, Md",
            Languages = "DE, FR, IT",
            NumberOfSearches = 18,
            DateLastSearch = DateTime.UtcNow.AddDays(-500),
            Created = DateTime.UtcNow,
            CreatedBy = "test@test.ch",
            Modified = DateTime.UtcNow,
            ModifiedBy = "test@test.ch",
        };

        await statisticRepository.Create(newStatistic);
        context.SaveChanges();

        var statisticCount = context.Statistics.Count();

        statisticRepository.Delete(newStatistic);
        context.SaveChanges();

        Assert.That(context.Statistics.Count(), Is.EqualTo(statisticCount - 1));
        Assert.That(context.Statistics, Is.Not.Contains(newStatistic));
    }

    [Test]
    public async Task DeleteStatistics_WhenCalled_ShouldRemoveStatisics()
    {
        var newStatistic = new Statistic
        {
            Id = 98,
            SearchText = "UralteSuche",
            SearchFields = "AB, Na, Md",
            Languages = "DE, FR, IT",
            NumberOfSearches = 18,
            DateLastSearch = DateTime.UtcNow,
            Created = DateTime.UtcNow,
            CreatedBy = "test@test.ch",
            Modified = DateTime.UtcNow,
            ModifiedBy = "test@test.ch",
        };

        await statisticRepository.Create(newStatistic);

        var anotherStatistic = new Statistic
        {
            Id = 99,
            SearchText = "UralteSuche",
            SearchFields = "AB, Na, Md",
            Languages = "DE, FR, IT",
            NumberOfSearches = 18,
            DateLastSearch = DateTime.UtcNow,
            Created = DateTime.UtcNow,
            CreatedBy = "test@test.ch",
            Modified = DateTime.UtcNow,
            ModifiedBy = "test@test.ch",
        };

        await statisticRepository.Create(anotherStatistic);
        context.SaveChanges();

        var statisticCount = context.Statistics.Count();

        var statisticArray = new Statistic[] { newStatistic, anotherStatistic };

        statisticRepository.DeleteStatistics(statisticArray);
        context.SaveChanges();

        Assert.That(context.Statistics.Count(), Is.EqualTo(statisticCount - 2));
        Assert.That(context.Statistics, Is.Not.Contains(newStatistic));
        Assert.That(context.Statistics, Is.Not.Contains(anotherStatistic));
    }

    [Test]
    public async Task GetBySearchQuery_WhenCalled_ShouldReturnData()
    {
        var searchQuery = new StatisticSearchQuery
        {
            Phrase = "DE,",
            FromDate = DateTime.UtcNow.AddDays(-365),
            ToDate = DateTime.UtcNow.AddDays(1)
        };

        var statistics = await statisticRepository.GetBySearchQuery(searchQuery);

        var first = statistics.First();
        var last = statistics.Last();

        Assert.That(statistics.Count, Is.EqualTo(4));

        Assert.That(first.Id, Is.EqualTo(5));
        Assert.That(first.SearchText, Is.EqualTo("Friedenspanzer"));

        Assert.That(last.Id, Is.EqualTo(4));
        Assert.That(last.SearchText, Is.EqualTo("Döppeköche"));
    }

    [Test]
    public async Task GetBySearchQuery_WithAscendingOrder_ShouldReturnOrderedData()
    {
        var searchQuery = new StatisticSearchQuery
        {
            Phrase = "DE,",
            FromDate = DateTime.UtcNow.AddDays(-365),
            ToDate = DateTime.UtcNow.AddDays(1),
            Sort = "searchText"
        };

        var statistics = await statisticRepository.GetBySearchQuery(searchQuery);

        var first = statistics.First();
        var last = statistics.Last();

        Assert.That(statistics.Count, Is.EqualTo(4));

        Assert.That(first.Id, Is.EqualTo(4));
        Assert.That(first.SearchText, Is.EqualTo("Döppeköche"));

        Assert.That(last.Id, Is.EqualTo(1));
        Assert.That(last.SearchText, Is.EqualTo("Rübezahl"));
    }

    [Test]
    public async Task GetBySearchQuery_WithDescendingOrder_ShouldReturnOrderedData()
    {
        var searchQuery = new StatisticSearchQuery
        {
            Phrase = "DE,",
            FromDate = DateTime.UtcNow.AddDays(-365),
            ToDate = DateTime.UtcNow.AddDays(1),
            Sort = "-searchText"
        };

        var statistics = await statisticRepository.GetBySearchQuery(searchQuery);

        var first = statistics.First();
        var last = statistics.Last();

        Assert.That(statistics.Count, Is.EqualTo(4));

        Assert.That(first.Id, Is.EqualTo(1));
        Assert.That(first.SearchText, Is.EqualTo("Rübezahl"));

        Assert.That(last.Id, Is.EqualTo(4));
        Assert.That(last.SearchText, Is.EqualTo("Döppeköche"));
    }

    [Test]
    public async Task GetObsoleteStatistic_WhenCalled_ShouldReturnData()
    {
        var veryOldSearch = new Statistic
        {
            Id = 6,
            SearchText = "UralteSuche",
            SearchFields = "AB, Na, Md",
            Languages = "DE, FR, IT",
            NumberOfSearches = 18,
            DateLastSearch = DateTime.UtcNow.AddDays(-500),
            Created = DateTime.UtcNow,
            CreatedBy = "test@test.ch",
            Modified = DateTime.UtcNow,
            ModifiedBy = "test@test.ch",
        };

        await statisticRepository.Create(veryOldSearch);

        var oldSearch = new Statistic
        {
            Id = 7,
            SearchText = "AlteSuche",
            SearchFields = "AB, Na, Md",
            Languages = "DE, FR, IT",
            NumberOfSearches = 18,
            DateLastSearch = DateTime.UtcNow.AddDays(-400),
            Created = DateTime.UtcNow,
            CreatedBy = "test@test.ch",
            Modified = DateTime.UtcNow,
            ModifiedBy = "test@test.ch",
        };

        await statisticRepository.Create(oldSearch);

        context.SaveChanges();

        var statistics = await statisticRepository.GetObsoleteStatistics();

        Assert.That(statistics.Count, Is.EqualTo(2));

        var currentStatistic = statistics.First();
        Assert.That(currentStatistic.Id, Is.EqualTo(7));

        currentStatistic = statistics.Last();
        Assert.That(currentStatistic.Id, Is.EqualTo(6));

        statisticRepository.Delete(veryOldSearch);
        statisticRepository.Delete(oldSearch);
    }
}
