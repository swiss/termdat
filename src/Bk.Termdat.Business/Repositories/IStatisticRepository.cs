using System.Collections.Generic;
using System.Threading.Tasks;
using Bk.Termdat.Business.Domain.Models;
using Bk.Termdat.Business.Resource.Models;

namespace Bk.Termdat.Business.Repositories;

public interface IStatisticRepository
{
    Task<Statistic> GetById(int id);
    Task<Statistic> GetBySummary(StatisticSummary summary);
    Task<IEnumerable<Statistic>> GetAll();
    Task<IEnumerable<Statistic>> GetBySearchQuery(StatisticSearchQuery searchQuery);
    Task<IEnumerable<Statistic>> GetObsoleteStatistics();

    Task Create(Statistic statistic);
    void Update(Statistic existing, Statistic update);
    void Delete(Statistic statistic);

    void DeleteStatistics(IEnumerable<Statistic> statistics);
}
