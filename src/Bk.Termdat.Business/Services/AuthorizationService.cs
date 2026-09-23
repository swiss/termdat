using System.Collections.Generic;
using System.Linq;
using Bk.Termdat.Business.Domain.Models;
using Bk.Termdat.Business.Services.Interfaces;
using Bk.Termdat.CrossCutting;
using Bk.Termdat.CrossCutting.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Bk.Termdat.Business.Services;

public class AuthorizationService : IAuthorizationService
{
    private readonly IHttpContextAccessor httpContextAccessor;
    private readonly AuthorizationOptions authorizationOptions;

    public AuthorizationService(IHttpContextAccessor httpContextAccessor, IOptions<AuthorizationOptions> authorizationOptions)
    {
        this.httpContextAccessor = httpContextAccessor;
        this.authorizationOptions = authorizationOptions.Value;
    }

    public bool IsAdmin()
    {
        return IsInRole(authorizationOptions.Admin);
    }

    public bool IsWriter()
    {
        return IsInRole(authorizationOptions.Writer);
    }

    public bool IsSuperUser()
    {
        return IsInRole(authorizationOptions.SuperUser);
    }

    public bool IsSuperReader()
    {
        return IsInRole(authorizationOptions.SuperReader);
    }

    public bool HasAccessToGlossary(int glossaryId)
    {
        return GetAuthorizedGlossaryIds().Contains(glossaryId);
    }

    public bool HasAccessToCollection(int collectionId)
    {
        return IsAdmin() || IsSuperUser() || GetAuthorizedCollectionIds().Contains(collectionId);
    }

    public bool CanEdit(Entry entry)
    {
        if (IsAdmin() || IsSuperUser())
        {
            return true;
        }

        if (!IsWriter())
        {
            return false;
        }

        var hasCollectionAccess = HasAccessToCollection(entry.CollectionId);
        var hasGlossaryAccess = entry.Glossaries?.Select(x => x.Id).Any(HasAccessToGlossary) ?? false;

        return hasCollectionAccess || hasGlossaryAccess;
    }

    public bool CanAddToWatchList(Entry entry)
    {
        if (CanEdit(entry))
        {
            return true;
        }

        if (IsSuperReader())
        {
            return HasAccessToCollection(entry.CollectionId);
        }

        return false;
    }

    public bool CanDelete(Entry entry)
    {
        return IsAdmin() || IsSuperUser();
    }

    public bool CanRestore(Entry entry)
    {
        return IsAdmin();
    }

    public IEnumerable<int> GetAuthorizedGlossaryIds()
    {
        var ids = FindClaim(CustomClaimTypes.GlossaryIds);

        return ParseClaim(ids);
    }

    public IEnumerable<int> GetAuthorizedCollectionIds()
    {
        var ids = FindClaim(CustomClaimTypes.CollectionIds);

        return ParseClaim(ids);
    }

    private static IEnumerable<int> ParseClaim(string value) => string.IsNullOrWhiteSpace(value) ? Enumerable.Empty<int>() : value.Split(",").Select(int.Parse);
    private string FindClaim(string claimType) => httpContextAccessor.HttpContext?.User.FindFirst(claimType)?.Value;
    private bool IsInRole(string role) => httpContextAccessor?.HttpContext?.User.IsInRole(role) ?? false;
}
