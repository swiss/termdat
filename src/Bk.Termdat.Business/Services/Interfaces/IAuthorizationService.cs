using System.Collections.Generic;
using Bk.Termdat.Business.Domain.Models;

namespace Bk.Termdat.Business.Services.Interfaces;

public interface IAuthorizationService
{
    bool IsAdmin();
    bool IsWriter();
    bool IsSuperUser();
    bool IsSuperReader();
    bool HasAccessToGlossary(int glossaryId);
    IEnumerable<int> GetAuthorizedGlossaryIds();
    bool HasAccessToCollection(int collectionId);
    IEnumerable<int> GetAuthorizedCollectionIds();
    bool CanEdit(Entry entry);
    bool CanDelete(Entry entry);
    bool CanRestore(Entry entry);
    bool CanAddToWatchList(Entry entry);
}
