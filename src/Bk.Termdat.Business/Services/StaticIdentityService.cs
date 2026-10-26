using Bk.Termdat.Business.Services.Interfaces;

namespace Bk.Termdat.Business.Services;

// Note:
// This service should only be used for background jobs where no user context (HttpContext) is available.
public class StaticIdentityService : IIdentityService
{
    public required string Email { get; init; }
    public required string LastName { get; init; }
    public required string FirstName { get; init; }
    public required string Role { get; init; }

    public string GetCurrentEmail()
    {
        return Email;
    }

    public string GetCurrentLastName()
    {
        return LastName;
    }

    public string GetCurrentFirstName()
    {
        return FirstName;
    }

    public string GetCurrentRole()
    {
        return Role;
    }
}
