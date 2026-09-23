using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bk.Termdat.Business.Domain.Models;
using Bk.Termdat.Business.Repositories;
using Bk.Termdat.Business.Resource.Models;
using Microsoft.EntityFrameworkCore;

namespace Bk.Termdat.Infrastructure.DataSource.Repositories;

public class StatisticRepository : IStatisticRepository
{
    private const int minimumSearches = 5;
    private const int numberOfDays = 365;

    private readonly DataContext context;

    public StatisticRepository(DataContext context)
    {
        this.context = context;
    }

    public async Task<Statistic> GetById(int id)
    {
        return await context.Statistics
            .SingleOrDefaultAsync(s => s.Id == id);
    }

    public async Task<IEnumerable<Statistic>> GetAll()
    {
        return await context.Statistics
            .Where(s => s.NumberOfSearches >= minimumSearches)
            .OrderByDescending(s => s.DateLastSearch)
            .ToListAsync();
    }

    public async Task<IEnumerable<Statistic>> GetBySearchQuery(StatisticSearchQuery searchQuery)
    {
        searchQuery.FromDate ??= DateTime.UtcNow.AddDays(-numberOfDays);
        searchQuery.ToDate ??= DateTime.UtcNow.AddDays(+1);

        var statistics = CreateQueryable(searchQuery);

        var databaseStatistics = await statistics.ToListAsync();

        if (!string.IsNullOrEmpty(searchQuery.Phrase))
        {
            databaseStatistics = databaseStatistics
                .Where(s => s.SearchText.ToLowerInvariant().Contains(searchQuery.Phrase.ToLowerInvariant()) ||
                s.NumberOfSearches.ToString().Contains(searchQuery.Phrase.ToLowerInvariant()) ||
                s.SearchFields.ToLowerInvariant().Contains(searchQuery.Phrase.ToLowerInvariant()) ||
                s.Languages.ToLowerInvariant().Contains(searchQuery.Phrase.ToLowerInvariant())).ToList();
        }

        return databaseStatistics;
    }

    public async Task<Statistic> GetBySummary(StatisticSummary summary)
    {
        return await context.Statistics
            .Where(s => s.SearchText.Equals(summary.SearchText))
            .Where(s => s.SearchFields.Equals(summary.SearchFields))
            .Where(s => s.Languages.Equals(summary.Languages))
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Statistic>> GetObsoleteStatistics()
    {
        return await context.Statistics
            .Where(s => s.DateLastSearch < DateTime.UtcNow.AddDays(-numberOfDays)).ToListAsync();
    }

    public async Task Create(Statistic statistic)
    {
        await context.Statistics.AddAsync(statistic);
    }

    public void Update(Statistic existing, Statistic update)
    {
        context.Entry(existing).CurrentValues.SetValues(update);
    }

    public void Delete(Statistic statistic)
    {
        context.Statistics.Remove(statistic);
    }

    public void DeleteStatistics(IEnumerable<Statistic> statistics)
    {
        context.Statistics.RemoveRange(statistics);
    }

    private IQueryable<Statistic> CreateQueryable(StatisticSearchQuery searchQuery)
    {
        IQueryable<Statistic> filteredStatistics;

        filteredStatistics = CreateBaseQueryableWithoutText(searchQuery, minimumSearches);

        filteredStatistics = OrderStatisticQueryable(filteredStatistics, searchQuery.Sort);

        return filteredStatistics;
    }

    private IQueryable<Statistic> CreateBaseQueryableWithoutText(StatisticSearchQuery searchQuery, int minimumSearches)
    {
        return context.Statistics
            .Where(s => s.NumberOfSearches >= minimumSearches)
            .Where(s => s.DateLastSearch >= searchQuery.FromDate)
            .Where(s => s.DateLastSearch <= searchQuery.ToDate);
    }

    private IQueryable<Statistic> OrderStatisticQueryable(IQueryable<Statistic> filteredStatistics, string sort)
    {
        IQueryable<Statistic> orderedStatistics = sort switch
        {
            "-dateLastSearch" => filteredStatistics.OrderByDescending(s => s.DateLastSearch),
            "dateLastSearch" => filteredStatistics.OrderBy(s => s.DateLastSearch),
            "-languages" => filteredStatistics.OrderByDescending(s => s.Languages),
            "languages" => filteredStatistics.OrderBy(s => s.Languages),
            "-searchText" => filteredStatistics.OrderByDescending(s => s.SearchText),
            "searchText" => filteredStatistics.OrderBy(s => s.SearchText),
            "-searchFields" => filteredStatistics.OrderByDescending(s => s.SearchFields),
            "searchFields" => filteredStatistics.OrderBy(s => s.SearchFields),
            "-numberOfSearches" => filteredStatistics.OrderByDescending(s => s.NumberOfSearches),
            "numberOfSearches" => filteredStatistics.OrderBy(s => s.NumberOfSearches),
            "-id" => filteredStatistics.OrderByDescending(s => s.Id),
            "id" => filteredStatistics.OrderBy(s => s.Id),
            _ => filteredStatistics.OrderByDescending(s => s.DateLastSearch),
        };
        return orderedStatistics;
    }
}
