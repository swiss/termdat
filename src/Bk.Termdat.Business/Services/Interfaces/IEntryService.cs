using System.Collections.Generic;
using System.Threading.Tasks;
using Bk.Termdat.Business.Resource.Models;

namespace Bk.Termdat.Business.Services.Interfaces;

public interface IEntryService
{
    Task<EntryModificationDetail> Create(EntryCreate entryCreate, string processingUser = null);
    Task<EntryModificationDetail> Update(EntryUpdate entryUpdate, string processingUser = null);
    Task<EntryDetail> GetDetail(int entryId, IEnumerable<int> sourceLanguageIds = null, IEnumerable<int> targetLanguageIds = null);
    Task<EntryModificationDetail> GetModificationDetail(int entryId);
    Task<EntryReferenceDetail> GetReferenceDetail(int entryId);
    Task<IEnumerable<EntryMultipleUpdateInfo>> DeleteMultiple(int[] ids, bool soft = true);
    Task<EntryMultipleUpdateResult> MultipleSearchAndReplace(EntrySearchAndReplaceUpdate update);
    Task<IEnumerable<EntryMultipleUpdateInfo>> RestoreMultiple(int[] ids);
    Task<uint> Lock(int id);
    Task<uint> Unlock(int id);
    Task UnlockExpiredEntries();
}
