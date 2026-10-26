using System;
using Bk.Termdat.Business.Services;
using Bk.Termdat.Business.Services.Interfaces;

namespace Bk.Termdat.Business.Extensions;

public static class AuthorizationServiceExtensions
{
    public static StaticAuthorizationService ConvertToStaticAuthorizationService(
        this IAuthorizationService authorizationService)
    {
        ArgumentNullException.ThrowIfNull(authorizationService);

        return new StaticAuthorizationService
        {
            IsAdminField = authorizationService.IsAdmin(),
            IsWriterField = authorizationService.IsWriter(),
            IsSuperUserField = authorizationService.IsSuperUser(),
            IsSuperReaderField = authorizationService.IsSuperReader()
        };
    }
}
