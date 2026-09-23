using Bk.Termdat.Business.Domain.Models;

namespace Bk.Termdat.Business.Domain.Projections;

public class EntityWithEntryCount<T> where T : Entity
{
    public T Entity { get; set; }
    public int EntryCount { get; set; }
}
