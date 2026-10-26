using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Bk.Termdat.Business.Domain;
using Bk.Termdat.Business.Domain.Models;
using Bk.Termdat.Business.Repositories;
using Bk.Termdat.Business.Resource.Models;
using Bk.Termdat.Business.Services.Interfaces;
using Bk.Termdat.Common.Resources;
using Bk.Termdat.CrossCutting.Exceptions;
using Microsoft.Extensions.Logging;

namespace Bk.Termdat.Business.Services;

public class StatisticService : IStatisticService
{
    private const string CreateUser = "statisticService";
    private readonly IMapper mapper;
    private readonly IStatisticRepository statisticRepository;
    private readonly IUnitOfWork unitOfWork;
    private readonly ILogger<StatisticService> logger;

    public StatisticService(IMapper mapper, IStatisticRepository statisticRepository, IUnitOfWork unitOfWork, ILogger<StatisticService> logger)
    {
        this.mapper = mapper;
        this.statisticRepository = statisticRepository;
        this.unitOfWork = unitOfWork;
        this.logger = logger;
    }

    public async Task<IEnumerable<StatisticSummary>> GetAll()
    {
        var statistics = await statisticRepository.GetAll();
        return mapper.Map<IEnumerable<StatisticSummary>>(statistics);
    }

    public async Task<StatisticSearchResult> GetBySearchQuery(StatisticSearchQuery searchQuery)
    {
        ArgumentNullException.ThrowIfNull(searchQuery);

        var statistics = await statisticRepository.GetBySearchQuery(searchQuery);

        var totalCount = statistics.Count();

        statistics = statistics.Skip(searchQuery.PageIndex * searchQuery.PageSize).Take(searchQuery.PageSize);

        var summaries = mapper.Map<IEnumerable<StatisticSummary>>(statistics);

        var result = new StatisticSearchResult()
        {
            Statistics = summaries,
            TotalCount = totalCount
        };

        return result;
    }

    public async Task<StatisticDetail> CreateOrUpdateStatistic(StatisticSummary summary)
    {
        ArgumentNullException.ThrowIfNull(summary);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Add not found search text to statistic: {SearchText}", summary.SearchText);
        }

        var existing = await GetBySummary(summary);

        if (existing == null)
        {
            return await Create(mapper.Map<StatisticCreate>(summary));
        }
        else
        {
            summary.Id = existing.Id;
            return await Update(mapper.Map<StatisticUpdate>(summary));
        }
    }

    public async Task<StatisticDetail> Create(StatisticCreate newStatistic)
    {
        var mappedStatistic = mapper.Map<Statistic>(newStatistic);
        mappedStatistic.NumberOfSearches = 1;
        mappedStatistic.Created = DateTime.UtcNow;
        mappedStatistic.CreatedBy = CreateUser;
        mappedStatistic.Modified = DateTime.UtcNow;
        mappedStatistic.ModifiedBy = CreateUser;

        await statisticRepository.Create(mappedStatistic);

        await unitOfWork.SaveChangesAsync();

        var statistic = await statisticRepository.GetById(mappedStatistic.Id);
        return mapper.Map<StatisticDetail>(statistic);
    }

    public async Task<StatisticDetail> Update(StatisticUpdate updatedStatistic)
    {
        ArgumentNullException.ThrowIfNull(updatedStatistic);

        var databaseStatistic = await statisticRepository.GetById(updatedStatistic.Id);

        if (databaseStatistic == null)
        {
            throw new EntityNotFoundException(string.Format(CultureInfo.InvariantCulture, BackendTexts.StatisticNotFound, updatedStatistic.Id));
        }

        var mappedStatistic = mapper.Map<Statistic>(updatedStatistic);

        mappedStatistic.NumberOfSearches = databaseStatistic.NumberOfSearches + 1;
        mappedStatistic.DateLastSearch = DateTime.UtcNow;
        mappedStatistic.Modified = DateTime.UtcNow;
        mappedStatistic.ModifiedBy = CreateUser;
        mappedStatistic.Created = databaseStatistic.Created;
        mappedStatistic.CreatedBy = databaseStatistic.CreatedBy;

        statisticRepository.Update(databaseStatistic, mappedStatistic);

        await unitOfWork.SaveChangesAsync();

        var statistic = await statisticRepository.GetById(databaseStatistic.Id);
        var statisticDetail = mapper.Map<StatisticDetail>(statistic);
        return statisticDetail;
    }

    public async Task Delete(int id)
    {
        var statistic = await statisticRepository.GetById(id);

        if (statistic == null)
        {
            throw new EntityNotFoundException(string.Format(CultureInfo.InvariantCulture, BackendTexts.StatisticNotFound, id));
        }

        statisticRepository.Delete(statistic);

        await unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteBySearchQuery(StatisticSearchQuery searchQuery)
    {
        var statistics = await statisticRepository.GetBySearchQuery(searchQuery);

        foreach (var statistic in statistics)
        {
            statisticRepository.Delete(statistic);
        }

        await unitOfWork.SaveChangesAsync();
    }

    private async Task<StatisticDetail> GetBySummary(StatisticSummary summary)
    {
        var statistic = await statisticRepository.GetBySummary(summary);
        var statisticDetail = mapper.Map<StatisticDetail>(statistic);

        return statisticDetail;
    }

    public async Task DeleteObsoleteStatistics()
    {
        var oldStatistics = await statisticRepository.GetObsoleteStatistics();

        statisticRepository.DeleteStatistics(oldStatistics);

        await unitOfWork.SaveChangesAsync();
    }
}
