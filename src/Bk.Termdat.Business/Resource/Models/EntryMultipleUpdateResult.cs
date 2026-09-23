using System.Collections.Generic;

namespace Bk.Termdat.Business.Resource.Models;

public class EntryMultipleUpdateResult
{
    public int EntryCount { get; set; }
    public int AffectedOccurrences { get; set; }
    public bool DataWasChanged { get; set; }
    public IList<EntryMultipleUpdateInfo> LockInfos { get; set; }
}
