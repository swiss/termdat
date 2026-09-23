using System.Collections.Generic;

namespace Bk.Termdat.Business.Resource.Models;

public class StatisticSearchResult
{
    public StatisticSearchResult()
    {
        Statistics = new List<StatisticSummary>();
    }

    public IEnumerable<StatisticSummary> Statistics { get; set; }
    public int TotalCount { get; set; }
}
