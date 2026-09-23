using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using Bk.Termdat.Gateway.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using OpenIdConnectOptions = Bk.Termdat.Gateway.Configuration.OpenIdConnectOptions;

namespace Bk.Termdat.Gateway.Services;

public class TokenService : ITokenService
{
    public const string TokenHttpClientName = "OpenIdConnectTokenEndpoint";

    // Enhancement over the DataReg reference implementation: serialize concurrent refresh attempts per
    // session so that rotating/single-use refresh tokens from the IdP don't get invalidated by a
    // "thundering herd" of parallel requests that all notice an expired access token at the same time.
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> RefreshLocks = new();

    private readonly IHttpClientFactory httpClientFactory;
    private readonly IHttpContextAccessor httpContextAccessor;
    private readonly ILogger<TokenService> logger;
    private readonly OpenIdConnectOptions oidcOptions;
    private readonly TimeProvider timeProvider;

    public TokenService(
        IHttpClientFactory httpClientFactory,
        IHttpContextAccessor httpContextAccessor,
        IOptions<OpenIdConnectOptions> oidcOptions,
        TimeProvider timeProvider,
        ILogger<TokenService> logger)
    {
        this.httpClientFactory = httpClientFactory;
        this.httpContextAccessor = httpContextAccessor;
        this.oidcOptions = oidcOptions.Value;
        this.timeProvider = timeProvider;
        this.logger = logger;
    }

    public async Task<string?> GetValidAccessToken()
    {
        try
        {
            var httpContext = httpContextAccessor.HttpContext;

            if (httpContext?.User.Identity?.IsAuthenticated is not true)
            {
                return null;
            }

            var accessToken = await httpContext.GetTokenAsync(OpenIdConnectParameterNames.AccessToken);
            if (IsTokenValid(accessToken))
            {
                return accessToken;
            }

            var lockKey = GetRefreshLockKey(httpContext.User);
            var refreshLock = RefreshLocks.GetOrAdd(lockKey, _ => new SemaphoreSlim(1, 1));

            await refreshLock.WaitAsync();
            try
            {
                // Re-check after acquiring the lock: another request for the same session may have
                // already refreshed the token while we were waiting.
                accessToken = await httpContext.GetTokenAsync(OpenIdConnectParameterNames.AccessToken);
                if (IsTokenValid(accessToken))
                {
                    return accessToken;
                }

                var refreshToken = await httpContext.GetTokenAsync(OpenIdConnectParameterNames.RefreshToken);
                if (string.IsNullOrWhiteSpace(refreshToken))
                {
                    logger.LogWarning("Access token expired but no refresh token available");
                    return null;
                }

                var newTokens = await RefreshToken(refreshToken);
                if (newTokens is null)
                {
                    logger.LogWarning("Failed to refresh token");
                    return null;
                }

                await UpdateAuthenticationProperties(httpContext, newTokens);

                return newTokens.AccessToken;
            }
            finally
            {
                refreshLock.Release();
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while trying to get valid access token");
            return null;
        }
    }

    public async Task SignOutUser()
    {
        try
        {
            logger.LogInformation("Signing out user");

            var httpContext = httpContextAccessor.HttpContext!;
            await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            await httpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during sign out user");
        }
    }

    private static string GetRefreshLockKey(ClaimsPrincipal user)
    {
        // Prefer a stable user identifier as the lock key so concurrent requests for the same user's
        // session serialize on the same semaphore. Fall back to the session-id claim, and finally to a
        // shared key (still correct, just less parallel-friendly) if neither is present.
        return user.FindFirst(ClaimTypes.NameIdentifier)?.Value
               ?? user.FindFirst("sub")?.Value
               ?? user.FindFirst("sid")?.Value
               ?? "unknown";
    }

    private bool IsTokenValid(string? accessToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                return false;
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var jwt = tokenHandler.ReadJwtToken(accessToken);

            return jwt.ValidTo > timeProvider.GetUtcNow().UtcDateTime.AddSeconds(30);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to parse access token");
            return false;
        }
    }

    private async Task<TokenResponse?> RefreshToken(string refreshToken)
    {
        try
        {
            using var httpClient = httpClientFactory.CreateClient(TokenHttpClientName);

            var tokenEndpoint = $"{oidcOptions.Authority}{oidcOptions.TokenEndpointPath}";
            var requestParams = new List<KeyValuePair<string, string>>
            {
                new("grant_type", "refresh_token"),
                new("refresh_token", refreshToken),
                new("client_id", oidcOptions.ClientId),
                new("client_secret", oidcOptions.ClientSecret)
            };

            var response = await httpClient.PostAsync(tokenEndpoint, new FormUrlEncodedContent(requestParams));

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                logger.LogWarning("Token refresh failed: {StatusCode} - {Content}", response.StatusCode, errorContent);
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(content, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
            });

            return tokenResponse;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error refreshing token");
            return null;
        }
    }

    private async Task UpdateAuthenticationProperties(HttpContext httpContext, TokenResponse tokens)
    {
        var authResult = await httpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        if (!authResult.Succeeded || authResult.Properties == null)
        {
            return;
        }

        authResult.Properties.UpdateTokenValue(OpenIdConnectParameterNames.AccessToken, tokens.AccessToken);
        authResult.Properties.UpdateTokenValue(OpenIdConnectParameterNames.RefreshToken, tokens.RefreshToken);

        await httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, httpContext.User,
            authResult.Properties);
    }
}
