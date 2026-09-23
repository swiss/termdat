using System;
using System.Collections.Generic;
using Bk.Termdat.Business.Domain.Models;
using Bk.Termdat.Business.Services.Interfaces;

namespace Bk.Termdat.Business.Services;

// Note:
// This service should only be used for background jobs where no user context (HttpContext) is available and no actual role checks are required (admin role job).
public class StaticAuthorizationService : IAuthorizationService
{
    public required bool IsAdminField { get; init; }
    public required bool IsWriterField { get; init; }
    public required bool IsSuperUserField { get; init; }
    public required bool IsSuperReaderField { get; init; }

    public bool IsAdmin()
    {
        return IsAdminField;
    }

    public bool IsWriter()
    {
        return IsWriterField;
    }

    public bool IsSuperUser()
    {
        return IsSuperUserField;
    }

    public bool IsSuperReader()
    {
        return IsSuperReaderField;
    }

    public bool HasAccessToGlossary(int glossaryId)
    {
        // Note: Admin role allows everything
        return IsAdminField;
    }

    public bool HasAccessToCollection(int collectionId)
    {
        return IsAdminField;
    }

    public bool CanEdit(Entry entry)
    {
        return IsAdminField;
    }

    public bool CanDelete(Entry entry)
    {
        return IsAdminField;
    }

    public bool CanRestore(Entry entry)
    {
        return IsAdminField;
    }

    public bool CanAddToWatchList(Entry entry)
    {
        return IsAdminField;
    }

    public IEnumerable<int> GetAuthorizedGlossaryIds()
    {
        throw new NotImplementedException();
    }

    public IEnumerable<int> GetAuthorizedCollectionIds()
    {
        throw new NotImplementedException();
    }
}
