using System.Collections.Generic;

namespace Bk.Termdat.Business.Resource.Models;

public class SearchPage
{
    public SearchPage()
    {
        SearchEntries = new List<EntrySummary>();
    }

    public IEnumerable<EntrySummary> SearchEntries { get; set; }
    public int TotalEntryCount { get; set; }
}
