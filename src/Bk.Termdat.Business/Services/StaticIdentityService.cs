using Bk.Termdat.Business.Services.Interfaces;

namespace Bk.Termdat.Business.Services;

// Note:
// This service should only be used for background jobs where no user context (HttpContext) is available.
public class StaticIdentityService : IIdentityService
{
    public required string CurrentEmail { get; init; }
    public required string CurrentLastName { get; init; }
    public required string CurrentFirstName { get; init; }
    public required string CurrentRole { get; init; }

    public string GetCurrentEmail()
    {
        return CurrentEmail;
    }

    public string GetCurrentLastName()
    {
        return CurrentLastName;
    }

    public string GetCurrentFirstName()
    {
        return CurrentFirstName;
    }

    public string GetCurrentRole()
    {
        return CurrentRole;
    }
}
