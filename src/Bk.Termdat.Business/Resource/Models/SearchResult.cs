using System.Collections.Generic;

namespace Bk.Termdat.Business.Resource.Models;

public class SearchResult
{
    public SearchResult()
    {
        EntryIds = new List<int>();
    }

    public IEnumerable<int> EntryIds { get; set; }
    public int TotalEntryCount { get; set; }
}
