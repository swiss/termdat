using Bk.Termdat.Business.Services;
using Bk.Termdat.Business.Services.Interfaces;

namespace Bk.Termdat.Business.Extensions;

public static class IdentityServiceExtensions
{
    public static StaticIdentityService ConvertToStaticIdentityService(this IIdentityService identityService)
    {
        return new StaticIdentityService
        {
            CurrentEmail = identityService.GetCurrentEmail(),
            CurrentLastName = identityService.GetCurrentLastName(),
            CurrentFirstName = identityService.GetCurrentFirstName(),
            CurrentRole = identityService.GetCurrentRole()
        };
    }
}
