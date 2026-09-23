using System.Collections.Generic;

namespace Bk.Termdat.Business.Messages;

public class SearchIndexEntryUpdateByEntryIdsEvent
{
    public IEnumerable<int> EntryIds { get; set; }
}
