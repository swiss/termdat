using System;

namespace Bk.Termdat.Business.Resource.Models;

public class StatisticUpdate : IStatisticModification
{
    public int Id { get; set; }
    public string SearchText { get; set; }
    public string Languages { get; set; }
    public string SearchFields { get; set; }
    public DateTime DateLastSearch { get; set; }
}
