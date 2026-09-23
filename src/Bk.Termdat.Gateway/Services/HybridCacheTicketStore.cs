using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Options;
using CookieOptions = Bk.Termdat.Gateway.Configuration.CookieOptions;

namespace Bk.Termdat.Gateway.Services;

public class HybridCacheTicketStore : ITicketStore
{
    private readonly HybridCache cache;
    private readonly CookieOptions cookieOptions;
    private readonly ILogger<HybridCacheTicketStore> logger;
    private readonly TimeProvider timeProvider;

    public HybridCacheTicketStore(
        ILogger<HybridCacheTicketStore> logger,
        HybridCache cache,
        TimeProvider timeProvider,
        IOptions<CookieOptions> cookieOptions)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(cache);
        ArgumentNullException.ThrowIfNull(timeProvider);
        ArgumentNullException.ThrowIfNull(cookieOptions);

        this.cache = cache;
        this.logger = logger;
        this.timeProvider = timeProvider;
        this.cookieOptions = cookieOptions.Value;
    }

    public async Task<string> StoreAsync(AuthenticationTicket ticket)
    {
        var key = Guid.NewGuid().ToString("N");

        try
        {
            await StoreTicketInCache(key, ticket);
            logger.LogDebug("Successfully stored authentication ticket with key: {Key}", key);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to store authentication ticket with key: {Key}", key);
        }

        // Always return the generated key, even if storing the ticket fails,
        // to ensure the session can proceed without interruption.
        return key;
    }

    public async Task RenewAsync(string key, AuthenticationTicket ticket)
    {
        try
        {
            await StoreTicketInCache(key, ticket);
            logger.LogDebug("Successfully renewed authentication ticket with key: {Key}", key);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to renew authentication ticket with key: {Key}", key);
        }
    }

    public async Task<AuthenticationTicket?> RetrieveAsync(string key)
    {
        try
        {
            logger.LogDebug("Attempting to retrieve authentication ticket for key: {Key}", key);

            var serializedTicket = await cache.GetOrCreateAsync(
                key,
                factory: _ =>
                {
                    logger.LogDebug("Authentication ticket with key '{Key}' was not found in cache", key);
                    return ValueTask.FromResult<byte[]?>(null);
                });

            if (serializedTicket is { Length: > 0 })
            {
                var ticket = TicketSerializer.Default.Deserialize(serializedTicket);
                logger.LogDebug("Successfully retrieved authentication ticket for key: {Key}", key);
                return ticket;
            }

            logger.LogDebug("Authentication ticket not found for key: {Key}", key);
            return null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve authentication ticket with key: {Key}", key);
            return null;
        }
    }

    public async Task RemoveAsync(string key)
    {
        try
        {
            await cache.RemoveAsync(key);
            logger.LogDebug("Successfully removed authentication ticket with key: {Key}", key);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to remove authentication ticket with key '{Key}'", key);
        }
    }

    private async Task StoreTicketInCache(string key, AuthenticationTicket ticket)
    {
        var lifetime = ticket.Properties.ExpiresUtc.HasValue
            ? ticket.Properties.ExpiresUtc.Value - timeProvider.GetUtcNow()
            : TimeSpan.Zero;

        if (lifetime <= TimeSpan.Zero)
        {
            // Enhancement over the DataReg reference implementation: rather than silently dropping the
            // ticket (which would leave the browser holding a cookie that references nothing, making the
            // user appear logged out on the very next request), fall back to the configured cookie
            // lifetime so the ticket is still stored with a sensible expiration.
            lifetime = TimeSpan.FromHours(cookieOptions.ExpireTimeSpanHours);
            logger.LogWarning(
                "Authentication ticket with key {Key} had no usable ExpiresUtc. Falling back to configured cookie lifetime ({Lifetime})",
                key, lifetime);
        }

        logger.LogDebug("Storing authentication ticket with key: {Key}, lifetime: {Lifetime}", key, lifetime);

        var serializedTicket = TicketSerializer.Default.Serialize(ticket);
        await cache.SetAsync(
            key,
            serializedTicket,
            new HybridCacheEntryOptions
            {
                Expiration = lifetime,
                LocalCacheExpiration = lifetime
            });
    }
}
