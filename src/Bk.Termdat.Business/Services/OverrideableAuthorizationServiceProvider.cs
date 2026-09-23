using System.Collections.Generic;
using Bk.Termdat.Business.Domain.Models;
using Bk.Termdat.Business.Services.Interfaces;

namespace Bk.Termdat.Business.Services;

public class OverrideableAuthorizationServiceProvider : IAuthorizationService
{
    public OverrideableAuthorizationServiceProvider(AuthorizationService authorizationService)
    {
        Provider = authorizationService;
    }

    public IAuthorizationService Provider { get; set; }

    public bool IsAdmin()
    {
        return Provider.IsAdmin();
    }

    public bool IsWriter()
    {
        return Provider.IsWriter();
    }

    public bool IsSuperUser()
    {
        return Provider.IsSuperUser();
    }

    public bool IsSuperReader()
    {
        return Provider.IsSuperReader();
    }

    public bool HasAccessToGlossary(int glossaryId)
    {
        return Provider.HasAccessToGlossary(glossaryId);
    }

    public bool HasAccessToCollection(int collectionId)
    {
        return Provider.HasAccessToCollection(collectionId);
    }

    public bool CanEdit(Entry entry)
    {
        return Provider.CanEdit(entry);
    }

    public bool CanDelete(Entry entry)
    {
        return Provider.CanDelete(entry);
    }

    public bool CanRestore(Entry entry)
    {
        return Provider.CanRestore(entry);
    }

    public bool CanAddToWatchList(Entry entry)
    {
        return Provider.CanAddToWatchList(entry);
    }

    public IEnumerable<int> GetAuthorizedGlossaryIds()
    {
        return Provider.GetAuthorizedGlossaryIds();
    }

    public IEnumerable<int> GetAuthorizedCollectionIds()
    {
        return Provider.GetAuthorizedCollectionIds();
    }
}
