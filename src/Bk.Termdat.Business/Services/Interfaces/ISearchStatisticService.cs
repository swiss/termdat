using System.Threading.Tasks;
using Bk.Termdat.Business.Resource.Models;

namespace Bk.Termdat.Business.Services.Interfaces;

public interface ISearchStatisticService
{
    Task<StatisticSummary> LogUnsuccessfulSearch(SearchFilterParameter searchFilterParameter, SearchFields searchFields);
}
