namespace Bk.Termdat.Business.Resource.Models;

public class EntryMultipleUpdateInfo
{
    public int? NotSoftDeletedEntryId { get; set; }
    public int? LockedEntryId { get; set; }
    public string LockedBy { get; set; }
}
