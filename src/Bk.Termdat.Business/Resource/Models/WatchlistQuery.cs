using System.Collections.Generic;

namespace Bk.Termdat.Business.Resource.Models;

public class WatchlistQuery
{
    public IEnumerable<int> EntryIds { get; set; }
}
