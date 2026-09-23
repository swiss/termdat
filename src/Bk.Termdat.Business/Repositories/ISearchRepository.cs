using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Bk.Termdat.Business.Domain.Projections;
using Bk.Termdat.Business.Resource.Models;

namespace Bk.Termdat.Business.Repositories;

public interface ISearchRepository
{
    SearchResult Search(SearchFilterParameter searchFilterParameter, SearchFields searchFields, bool includeDeleted = false, bool includeDeactivatedCollectionEntries = false);
    IEnumerable<string> Recommend(string searchPhrase, IEnumerable<int> languageIds = null, bool includeDeleted = false, bool includeDeactivatedCollectionEntries = false);
    Task ImportEntries(IAsyncEnumerable<IndexEntry> entries, CancellationToken stoppingToken);
    void AddEntries(IEnumerable<IndexEntry> entries);
    void UpdateEntries(IEnumerable<IndexEntry> entries);
    void DeleteEntries(IEnumerable<int> entryIds);

}
