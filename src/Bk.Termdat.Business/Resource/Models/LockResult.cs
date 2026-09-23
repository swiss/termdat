namespace Bk.Termdat.Business.Resource.Models;

public class LockResult
{
    public bool IsLocked { get; set; }
    public uint ConcurrencyToken { get; set; }
}
