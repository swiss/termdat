using System.Collections.Generic;
using System.Threading.Tasks;
using Bk.Termdat.Business.Domain.Models;
using Bk.Termdat.Business.Domain.Projections;

namespace Bk.Termdat.Business.Repositories;

public interface IEntryRepository
{
    Task<int> Count();
    Task<IEnumerable<Entry>> GetByIdsForSearchResultsAdminOrWriterOrSuperReader(IEnumerable<int> ids);
    Task<IEnumerable<Entry>> GetByIdsForSearchResults(IEnumerable<int> ids);
    Task<IEnumerable<Entry>> GetByIdsForWatchlist(IEnumerable<int> ids);
    Task<Entry> GetByIdForEntryDetail(int id);
    IAsyncEnumerable<IndexEntry> GetPageForIndex(int skip, int take);
    Task Create(Entry entry);
    void Update(Entry existing, Entry update);
    void UpdateGlossaries(Entry existing, Entry update);
    void UpdateRelatedCollections(Entry existing, Entry update);
    void UpdateAdminInfos(Entry existing, Entry update);
    void UpdateWithoutRelations(Entry existing, Entry update);
    void UpdateMandatoryHeaderFields(Entry existing, Entry update);
    void Delete(Entry entry);
    Task<Entry> GetById(int id);
    Task<IEnumerable<IndexEntry>> GetByIdForIndex(int entryId);
    Task<IEnumerable<IndexEntry>> GetByIdsForIndex(IEnumerable<int> ids);
    Task<IEnumerable<int>> GetIdsByCollectionId(int collectionId);
    Task<IEnumerable<int>> GetIdsByGlossaryId(int glossaryId);
    Task<Entry> GetByIdForUpdate(int id);
    Task<Entry> GetByExternalSystemId(string externalSystemId);
    Task<IEnumerable<Entry>> GetByIdsForUpdate(IEnumerable<int> ids);
    Task<Entry> GetByIdForReferenceDetail(int id);
    Task<IEnumerable<Entry>> GetByIds(IEnumerable<int> ids);
    Task<IEnumerable<Entry>> GetByIdsForApiDetail(IEnumerable<int> ids);
    Task<IEnumerable<Entry>> GetByIdsForExport(IEnumerable<int> ids);
    Task<IEnumerable<Entry>> GetByIdsForApiSummary(IEnumerable<int> ids);
    Task<IEnumerable<int>> GetIdsByConceptPositionIds(IEnumerable<int> positionIds);
    Task<IEnumerable<int>> GetIdsByAdminInfoId(int adminInfoId);
    Task<IEnumerable<Entry>> GetByIdsForReplace(IEnumerable<int> ids, string searchField, string searchText);
    Task<IEnumerable<Entry>> GetExpiredLockedEntries(int expirationMinutes = 120);
}
