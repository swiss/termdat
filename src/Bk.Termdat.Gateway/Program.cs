using System.IdentityModel.Tokens.Jwt;
using Bk.Termdat.Gateway.Configuration;
using Bk.Termdat.Gateway.Services;
using Bk.Termdat.Gateway.Transforms;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Serilog;
using Swiss.FCh.Utils.Extensions;
using CookieOptions = Bk.Termdat.Gateway.Configuration.CookieOptions;
using OpenIdConnectOptions = Bk.Termdat.Gateway.Configuration.OpenIdConnectOptions;
using SameSiteMode = Microsoft.AspNetCore.Http.SameSiteMode;

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.AddSerilog("Bk.TERMDAT.Gateway");

    // Clear default claim mappings to preserve OIDC standard claim names
    JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
    JsonWebTokenHandler.DefaultInboundClaimTypeMap.Clear();

    var oidcConfig = builder.Services
        .AddValidatedOptions<OpenIdConnectOptions>(builder.Configuration, OpenIdConnectOptions.SectionKey)
        .Get<OpenIdConnectOptions>()!;

    var cookieConfig = builder.Services
        .AddValidatedOptions<CookieOptions>(builder.Configuration, CookieOptions.SectionKey)
        .Get<CookieOptions>()!;

    var gatewayConfig = builder.Services
        .AddValidatedOptions<GatewayOptions>(builder.Configuration, GatewayOptions.SectionKey)
        .Get<GatewayOptions>()!;

    builder.Services.AddSingleton(TimeProvider.System);

    builder.WebHost.ConfigureKestrel(options =>
    {
        options.Limits.MaxRequestBodySize = 60 * 1024 * 1024;
    });

    // Add HybridCache - level 1 only
    builder.Services.AddHybridCache(options =>
    {
        options.DefaultEntryOptions = new HybridCacheEntryOptions
        {
            // Set short default expiration to quickly expire null results from GetOrCreateAsync
            Expiration = TimeSpan.FromSeconds(1),
            LocalCacheExpiration = TimeSpan.FromSeconds(1)
        };
    });

    builder.Services.AddSingleton<ITicketStore, HybridCacheTicketStore>();
    builder.Services
        .AddSingleton<IPostConfigureOptions<CookieAuthenticationOptions>, CookieAuthenticationPostConfigureOptions>();

    builder.Services.AddAuthentication(options =>
        {
            options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
        })
        .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
        {
            // Note: Do not set LoginPath or LogoutPath — it causes redirect loops.
            // Unauthenticated signin-oidc callbacks trigger re-login, creating an infinite loop.
            options.Cookie.Name = cookieConfig.Name;
            options.Cookie.HttpOnly = cookieConfig.HttpOnly;
            options.Cookie.SecurePolicy = Enum.Parse<CookieSecurePolicy>(cookieConfig.SecurePolicy);
            options.Cookie.SameSite = Enum.Parse<SameSiteMode>(cookieConfig.SameSite);
            options.ExpireTimeSpan = TimeSpan.FromHours(cookieConfig.ExpireTimeSpanHours);
            options.SlidingExpiration = cookieConfig.SlidingExpiration;
        })
        .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
        {
            options.Authority = oidcConfig.Authority;
            options.ClientId = oidcConfig.ClientId;
            options.ClientSecret = oidcConfig.ClientSecret;
            options.ResponseType = oidcConfig.ResponseType;
            options.SaveTokens = oidcConfig.SaveTokens;
            options.CallbackPath = oidcConfig.CallbackPath;
            options.SignedOutCallbackPath = oidcConfig.SignedOutCallbackPath;
            options.RequireHttpsMetadata = oidcConfig.RequireHttpsMetadata;

            if (!string.IsNullOrWhiteSpace(oidcConfig.Scope))
            {
                options.Scope.Clear();
                foreach (var scope in oidcConfig.Scope.Split(' ', StringSplitOptions.RemoveEmptyEntries))
                {
                    options.Scope.Add(scope);
                }
            }
        });

    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("RequireAuthentication", policy =>
        {
            policy.RequireAuthenticatedUser();
        });
    });

    builder.Services.AddHttpClient(TokenService.TokenHttpClientName, client =>
    {
        client.BaseAddress = new Uri(oidcConfig.Authority);
    });
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<ITokenService, TokenService>();

    // Add YARP services
    builder.Services.AddReverseProxy()
        .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
        .AddTransforms(builderContext =>
        {
            if (builderContext.Route.RouteId == "api-route")
            {
                // Replace cookie with validated/refreshed access token for API requests
                builderContext.RequestTransforms.Add(new ApiAuthorizationRequestTransform());
            }
        });

    builder.Services.AddHealthChecks();

    // Configure forwarded headers to trust scheme/host from proxy
    builder.Services.Configure<ForwardedHeadersOptions>(options =>
    {
        options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto |
                                   ForwardedHeaders.XForwardedHost;

        options.KnownIPNetworks.Clear();
        options.KnownProxies.Clear();
    });

    var app = builder.Build();

    app.UseForwardedHeaders();

    app.UseAuthentication();
    app.UseAuthorization();

    // Add authentication endpoints
    app.MapGet(gatewayConfig.LoginEndpoint, async (HttpContext context, string? returnUrl = null) =>
    {
        // Validates if a return URL is safe to use for redirects.
        // This helps mitigate attacks where users could be redirected to malicious external sites
        // (e.g., /auth/login?returnUrl=https://evil.com).
        var isValidReturnUrl = Uri.IsWellFormedUriString(returnUrl, UriKind.Relative);

        // Redirect to intended destination if already authenticated
        if (context.User.Identity?.IsAuthenticated is true)
        {
            var redirectUrl = isValidReturnUrl ? returnUrl! : "/";
            return Results.Redirect(redirectUrl);
        }

        if (isValidReturnUrl)
        {
            await context.ChallengeAsync(OpenIdConnectDefaults.AuthenticationScheme,
                new AuthenticationProperties { RedirectUri = returnUrl });
        }
        else
        {
            await context.ChallengeAsync(OpenIdConnectDefaults.AuthenticationScheme);
        }

        return Results.Empty;
    });

    app.MapGet(gatewayConfig.LogoutEndpoint, async (HttpContext context, string? returnUrl = null) =>
    {
        // Sign out locally first
        await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        // Then sign out from the OIDC provider with proper redirect
        var authProps = new AuthenticationProperties
        {
            RedirectUri = Uri.IsWellFormedUriString(returnUrl, UriKind.Relative) ? returnUrl : "/"
        };

        await context.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme, authProps);
        return Results.Empty;
    });

    app.MapGet(gatewayConfig.UserEndpoint, async (HttpContext context, ITokenService tokenService) =>
    {
        if (context.User.Identity?.IsAuthenticated is not true)
        {
            return Results.Unauthorized();
        }

        var accessToken = await tokenService.GetValidAccessToken();
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            await tokenService.SignOutUser();
            return Results.Unauthorized();
        }

        return Results.Ok(new
        {
            IsAuthenticated = true,
            Claims = context.User.Claims.Select(c => new { c.Type, c.Value })
        });
    });

    app.MapReverseProxy();

    app.MapHealthChecks("/health");

    Log.Information("Starting web host: Bk.Termdat.Gateway");
    await app.RunAsync();

    return 0;
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
    return 1;
}
finally
{
    await Log.CloseAndFlushAsync();
}
