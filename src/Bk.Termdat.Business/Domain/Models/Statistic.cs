using System;

namespace Bk.Termdat.Business.Domain.Models;

// Statistiken
public class Statistic : EntityChangeTracking
{
    public string SearchText { get; set; }

    public int NumberOfSearches { get; set; }

    public string Languages { get; set; }

    public string SearchFields { get; set; }

    public DateTime DateLastSearch { get; set; }
}
