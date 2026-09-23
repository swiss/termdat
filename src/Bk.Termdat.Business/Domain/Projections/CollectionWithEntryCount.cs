using Bk.Termdat.Business.Domain.Models;

namespace Bk.Termdat.Business.Domain.Projections;

public class CollectionWithEntryCount
{
    public Collection Collection { get; set; }
    public int EntryCount { get; set; }
}
