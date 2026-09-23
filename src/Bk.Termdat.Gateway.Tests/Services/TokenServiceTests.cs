using System;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Bk.Termdat.Gateway.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using NUnit.Framework;
using OpenIdConnectOptions = Bk.Termdat.Gateway.Configuration.OpenIdConnectOptions;

namespace Bk.Termdat.Gateway.Tests.Services;

[TestFixture]
internal class TokenServiceTests
{
    private IHttpClientFactory httpClientFactory = null!;
    private IHttpContextAccessor httpContextAccessor = null!;
    private TokenService tokenService = null!;

    [SetUp]
    public void SetUp()
    {
        httpClientFactory = Substitute.For<IHttpClientFactory>();
        httpContextAccessor = Substitute.For<IHttpContextAccessor>();

        var oidcOptions = Options.Create(new OpenIdConnectOptions
        {
            Authority = "https://idp.example.test",
            ClientId = "client",
            ClientSecret = "secret",
            CallbackPath = "/auth/signin-oidc",
            SignedOutCallbackPath = "/auth/signout-callback-oidc",
            ResponseType = "code",
            SaveTokens = true,
            RequireHttpsMetadata = true,
            TokenEndpointPath = "/protocol/openid-connect/token",
            Scope = "oidc"
        });

        tokenService = new TokenService(
            httpClientFactory,
            httpContextAccessor,
            oidcOptions,
            TimeProvider.System,
            NullLogger<TokenService>.Instance);
    }

    [Test]
    public async Task GetValidAccessToken_UserNotAuthenticated_ReturnsNullWithoutCallingTokenEndpoint()
    {
        var httpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity()) };
        httpContextAccessor.HttpContext.Returns(httpContext);

        var result = await tokenService.GetValidAccessToken();

        Assert.That(result, Is.Null);
        httpClientFactory.DidNotReceive().CreateClient(Arg.Any<string>());
    }

    [Test]
    public async Task GetValidAccessToken_NoHttpContext_ReturnsNull()
    {
        httpContextAccessor.HttpContext.Returns(default(HttpContext));

        var result = await tokenService.GetValidAccessToken();

        Assert.That(result, Is.Null);
    }
}
