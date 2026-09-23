using System;

namespace Bk.Termdat.Business.Resource.Models;

public class StatisticCreate : IStatisticModification
{
    public string SearchText { get; set; }
    public string Languages { get; set; }
    public string SearchFields { get; set; }
    public DateTime DateLastSearch { get; set; }
}
