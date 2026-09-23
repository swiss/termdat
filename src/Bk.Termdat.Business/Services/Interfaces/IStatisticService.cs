using System.Collections.Generic;
using System.Threading.Tasks;
using Bk.Termdat.Business.Resource.Models;

namespace Bk.Termdat.Business.Services.Interfaces;

public interface IStatisticService
{
    Task<StatisticDetail> CreateOrUpdateStatistic(StatisticSummary summary);
    Task<IEnumerable<StatisticSummary>> GetAll();
    Task<StatisticSearchResult> GetBySearchQuery(StatisticSearchQuery searchQuery);

    Task<StatisticDetail> Create(StatisticCreate newStatistic);
    Task<StatisticDetail> Update(StatisticUpdate updatedStatistic);
    Task Delete(int id);
    Task DeleteBySearchQuery(StatisticSearchQuery searchQuery);
    Task DeleteObsoleteStatistics();
}
