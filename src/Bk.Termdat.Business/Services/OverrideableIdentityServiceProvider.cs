using Bk.Termdat.Business.Services.Interfaces;

namespace Bk.Termdat.Business.Services;

public class OverrideableIdentityServiceProvider : IIdentityService
{
    public OverrideableIdentityServiceProvider(IdentityService identityService)
    {
        Provider = identityService;
    }

    public IIdentityService Provider { get; set; }

    public string GetCurrentEmail()
    {
        return Provider.GetCurrentEmail();
    }

    public string GetCurrentLastName()
    {
        return Provider.GetCurrentLastName();
    }

    public string GetCurrentFirstName()
    {
        return Provider.GetCurrentFirstName();
    }

    public string GetCurrentRole()
    {
        return Provider.GetCurrentRole();
    }
}
