using System;
using Bk.Termdat.Business.Services;
using Bk.Termdat.Business.Services.Interfaces;

namespace Bk.Termdat.Business.Extensions;

public static class IdentityServiceExtensions
{
    public static StaticIdentityService ConvertToStaticIdentityService(this IIdentityService identityService)
    {
        ArgumentNullException.ThrowIfNull(identityService);

        return new StaticIdentityService
        {
            Email = identityService.GetCurrentEmail(),
            LastName = identityService.GetCurrentLastName(),
            FirstName = identityService.GetCurrentFirstName(),
            Role = identityService.GetCurrentRole()
        };
    }
}
