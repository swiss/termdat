using Bk.Termdat.Business.Domain.Models;

namespace Bk.Termdat.Business.Domain.Projections;

public class AdminInfoWithEntryCount
{
    public AdminInfo AdminInfo { get; set; }
    public int EntryCount { get; set; }
}
