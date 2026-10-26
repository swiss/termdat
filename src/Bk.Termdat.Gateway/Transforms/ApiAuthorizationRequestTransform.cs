using Bk.Termdat.Gateway.Services;
using Microsoft.Net.Http.Headers;
using Yarp.ReverseProxy.Transforms;

namespace Bk.Termdat.Gateway.Transforms;

/// <summary>
/// Strips any browser cookie from the proxied request to the api-route and replaces the
/// Authorization header with a validated/refreshed Bearer access token. Extracted from the
/// YARP transform pipeline (rather than an inline lambda in Program.cs, as in the DataReg
/// reference implementation) so it can be unit tested directly.
/// </summary>
public class ApiAuthorizationRequestTransform : RequestTransform
{
    public override async ValueTask ApplyAsync(RequestTransformContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        context.ProxyRequest.Headers.Remove(HeaderNames.Cookie);
        context.ProxyRequest.Headers.Remove(HeaderNames.Authorization);

        if (context.HttpContext.User.Identity?.IsAuthenticated is not true)
        {
            return;
        }

        var tokenService = context.HttpContext.RequestServices.GetRequiredService<ITokenService>();
        var accessToken = await tokenService.GetValidAccessToken();

        if (string.IsNullOrWhiteSpace(accessToken))
        {
            // Note:
            // Sign out from both cookie and OIDC schemes.
            // This is critical to prevent users from being in an inconsistent authentication state
            // where they appear authenticated to the gateway (valid cookie) but can't access
            // backend services (expired/invalid access token with no refresh capability).
            // Automatic sign-out ensures a clean authentication state and forces re-authentication.
            await tokenService.SignOutUser();
            return;
        }
        context.ProxyRequest.Headers.Add(HeaderNames.Authorization, $"Bearer {accessToken}");
    }
}
