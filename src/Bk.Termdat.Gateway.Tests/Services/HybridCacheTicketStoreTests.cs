using System;
using System.Threading.Tasks;
using Bk.Termdat.Gateway.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using CookieOptions = Bk.Termdat.Gateway.Configuration.CookieOptions;

namespace Bk.Termdat.Gateway.Tests.Services;

[TestFixture]
internal class HybridCacheTicketStoreTests
{
    private const int ConfiguredExpireTimeSpanHours = 8;

    private HybridCache cache = null!;
    private FakeTimeProvider timeProvider = null!;
    private HybridCacheTicketStore ticketStore = null!;

    [SetUp]
    public void SetUp()
    {
        var services = new ServiceCollection();
        services.AddHybridCache();
        cache = services.BuildServiceProvider().GetRequiredService<HybridCache>();

        timeProvider = new FakeTimeProvider(DateTimeOffset.UtcNow);

        var cookieOptions = Options.Create(new CookieOptions
        {
            Name = "Test.Auth",
            HttpOnly = true,
            SecurePolicy = "SameAsRequest",
            SameSite = "Lax",
            ExpireTimeSpanHours = ConfiguredExpireTimeSpanHours,
            SlidingExpiration = true
        });

        ticketStore = new HybridCacheTicketStore(NullLogger<HybridCacheTicketStore>.Instance, cache, timeProvider, cookieOptions);
    }

    [Test]
    public async Task StoreAsync_WithExpiresUtcInFuture_TicketCanBeRetrieved()
    {
        var ticket = CreateTicket(timeProvider.GetUtcNow().AddHours(1));

        var key = await ticketStore.StoreAsync(ticket);
        var retrieved = await ticketStore.RetrieveAsync(key);

        Assert.That(retrieved, Is.Not.Null);
    }

    [Test]
    public async Task StoreAsync_WithoutExpiresUtc_FallsBackToConfiguredCookieLifetimeInsteadOfDroppingTicket()
    {
        var ticket = CreateTicket(expiresUtc: null);

        var key = await ticketStore.StoreAsync(ticket);
        var retrieved = await ticketStore.RetrieveAsync(key);

        // Without the fallback, the ticket would silently not be stored (lifetime computed as
        // TimeSpan.Zero) and RetrieveAsync would return null, making the user appear logged out.
        Assert.That(retrieved, Is.Not.Null);
    }

    [Test]
    public async Task StoreAsync_WithExpiresUtcInThePast_FallsBackToConfiguredCookieLifetimeInsteadOfDroppingTicket()
    {
        var ticket = CreateTicket(timeProvider.GetUtcNow().AddHours(-1));

        var key = await ticketStore.StoreAsync(ticket);
        var retrieved = await ticketStore.RetrieveAsync(key);

        Assert.That(retrieved, Is.Not.Null);
    }

    private static AuthenticationTicket CreateTicket(DateTimeOffset? expiresUtc)
    {
        var identity = new System.Security.Claims.ClaimsIdentity(authenticationType: "TestAuth");
        var principal = new System.Security.Claims.ClaimsPrincipal(identity);
        var properties = new AuthenticationProperties { ExpiresUtc = expiresUtc };

        return new AuthenticationTicket(principal, properties, "TestScheme");
    }

    private sealed class FakeTimeProvider : TimeProvider
    {
        private readonly DateTimeOffset utcNow;

        public FakeTimeProvider(DateTimeOffset utcNow)
        {
            this.utcNow = utcNow;
        }

        public override DateTimeOffset GetUtcNow() => utcNow;
    }
}
