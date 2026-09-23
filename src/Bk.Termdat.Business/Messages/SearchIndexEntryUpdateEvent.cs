using System.Collections.Generic;
using Bk.Termdat.Business.Domain.Projections;

namespace Bk.Termdat.Business.Messages;

public class SearchIndexEntryUpdateEvent
{
    public IEnumerable<IndexEntry> IndexEntries { get; set; }
}
