using System;

namespace Bk.Termdat.Business.Resource.Models;

public class StatisticSummary
{
    public int? Id { get; set; }

    public string SearchText { get; set; }

    public int NumberOfSearches { get; set; }

    public string Languages { get; set; }

    public string SearchFields { get; set; }

    public DateTime DateLastSearch { get; set; }
}
