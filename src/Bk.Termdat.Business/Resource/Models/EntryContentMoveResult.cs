using System.Collections.Generic;

namespace Bk.Termdat.Business.Resource.Models;

public class EntryContentMoveResult
{
    public IEnumerable<int> NonAffectedEntryIds { get; set; }
    public bool DataWasChanged { get; set; }
    public IList<EntryMultipleUpdateInfo> LockInfos { get; set; }
}
