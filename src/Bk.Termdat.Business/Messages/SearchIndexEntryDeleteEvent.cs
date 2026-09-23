using System.Collections.Generic;

namespace Bk.Termdat.Business.Messages;

public class SearchIndexEntryDeleteEvent
{
    public IEnumerable<int> EntryIds { get; set; }
}
