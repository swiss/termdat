using System.Collections.Generic;
using System.Threading.Tasks;
using Bk.Termdat.Business.Domain.Projections;

namespace Bk.Termdat.Business.Clients;

public interface ISearchQueueClient
{
    Task QueueIndexAdd(IEnumerable<IndexEntry> indexEntries);
    Task QueueIndexUpdate(IEnumerable<IndexEntry> indexEntries);
    Task QueueIndexUpdateByIds(IEnumerable<int> ids);
    Task QueueIndexDelete(IEnumerable<int> entryIds);
}
