using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;

namespace Bk.Termdat.Gateway.Configuration;

public class CookieAuthenticationPostConfigureOptions : IPostConfigureOptions<CookieAuthenticationOptions>
{
    private readonly IServiceProvider serviceProvider;

    public CookieAuthenticationPostConfigureOptions(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
    }

    public void PostConfigure(string? name, CookieAuthenticationOptions options)
    {
        if (name == CookieAuthenticationDefaults.AuthenticationScheme)
        {
            options.SessionStore = serviceProvider.GetRequiredService<ITicketStore>();
        }
    }
}
