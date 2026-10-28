using System;

namespace Bk.Termdat.Business.Resource.Models;

public class StatisticSearchQuery
{
    public StatisticSearchQuery()
    {
        PageIndex = 0;
        PageSize = 100;
        Sort = "DateLastSearch";
    }

    public string? Phrase { get; set; }
    public int PageIndex { get; set; }
    public int PageSize { get; set; }
    public string? Sort { get; set; }

    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}
