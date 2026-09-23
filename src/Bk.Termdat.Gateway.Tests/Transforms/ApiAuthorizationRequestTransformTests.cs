using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Bk.Termdat.Gateway.Services;
using Bk.Termdat.Gateway.Transforms;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
using NSubstitute;
using NUnit.Framework;
using Yarp.ReverseProxy.Transforms;

namespace Bk.Termdat.Gateway.Tests.Transforms;

[TestFixture]
internal class ApiAuthorizationRequestTransformTests
{
    private ITokenService tokenService = null!;
    private ApiAuthorizationRequestTransform transform = null!;

    [SetUp]
    public void SetUp()
    {
        tokenService = Substitute.For<ITokenService>();
        transform = new ApiAuthorizationRequestTransform();
    }

    [Test]
    public async Task ApplyAsync_UserNotAuthenticated_DoesNotTouchHeaders()
    {
        var context = CreateContext(isAuthenticated: false);
        context.ProxyRequest.Headers.Add(HeaderNames.Cookie, "some-cookie=value");

        await transform.ApplyAsync(context);

        Assert.That(context.ProxyRequest.Headers.Authorization, Is.Null);
        Assert.That(context.ProxyRequest.Headers.Contains(HeaderNames.Cookie), Is.False);
        await tokenService.DidNotReceive().GetValidAccessToken();
    }

    [Test]
    public async Task ApplyAsync_ValidAccessToken_SetsBearerAuthorizationHeaderAndRemovesCookie()
    {
        var context = CreateContext(isAuthenticated: true);
        context.ProxyRequest.Headers.Add(HeaderNames.Cookie, "some-cookie=value");
        tokenService.GetValidAccessToken().Returns("the-access-token");

        await transform.ApplyAsync(context);

        Assert.That(context.ProxyRequest.Headers.Contains(HeaderNames.Cookie), Is.False);
        Assert.That(context.ProxyRequest.Headers.GetValues(HeaderNames.Authorization).Single(), Is.EqualTo("Bearer the-access-token"));
        Assert.That(context.HttpContext.Response.StatusCode, Is.Not.EqualTo(StatusCodes.Status401Unauthorized));
    }

    [Test]
    public async Task ApplyAsync_NoValidAccessToken_SignsOutAndForwardsWithoutAuthorizationHeader()
    {
        var context = CreateContext(isAuthenticated: true);
        tokenService.GetValidAccessToken().Returns(default(string));

        await transform.ApplyAsync(context);

        await tokenService.Received(1).SignOutUser();
        Assert.That(context.HttpContext.Response.StatusCode, Is.Not.EqualTo(StatusCodes.Status401Unauthorized));
        Assert.That(context.ProxyRequest.Headers.Authorization, Is.Null);
    }

    private RequestTransformContext CreateContext(bool isAuthenticated)
    {
        var services = new ServiceCollection();
        services.AddSingleton(tokenService);
        var serviceProvider = services.BuildServiceProvider();

        var identity = isAuthenticated
            ? new ClaimsIdentity(authenticationType: "TestAuth")
            : new ClaimsIdentity();

        var httpContext = new DefaultHttpContext
        {
            RequestServices = serviceProvider,
            User = new ClaimsPrincipal(identity)
        };

        return new RequestTransformContext
        {
            HttpContext = httpContext,
            ProxyRequest = new HttpRequestMessage(),
            Path = "/api/test",
            Query = new QueryTransformContext(httpContext.Request)
        };
    }
}
