using System.Collections.Generic;

namespace Bk.Termdat.Business.Resource.Models;

public class EntrySourceUpdateResult
{
    public bool DataWasChanged { get; set; }
    public IEnumerable<int> NotFullyUpdatedEntryIds { get; set; }
    public IList<EntryMultipleUpdateInfo> LockInfos { get; set; }
}
