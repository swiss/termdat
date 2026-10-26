using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Bk.Termdat.Business.Services;
using Bk.Termdat.CrossCutting.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using NSubstitute;
using NSubstitute.ClearExtensions;
using NSubstitute.ReturnsExtensions;
using NUnit.Framework;

namespace Bk.Termdat.Business.Tests.Services;

[TestFixture]
internal class IdentityServiceTests
{
    private IdentityService identityService;
    private AuthorizationOptions authorizationOptions;

    private readonly IHttpContextAccessor httpContextAccessorMock = Substitute.For<IHttpContextAccessor>();
    private readonly IOptions<AuthorizationOptions> options = Substitute.For<IOptions<AuthorizationOptions>>();

    private const string SuperUser = nameof(AuthorizationOptions.SuperUser);
    private const string SuperReader = nameof(AuthorizationOptions.SuperReader);
    private const string Admin = nameof(AuthorizationOptions.Admin);
    private const string Writer = nameof(AuthorizationOptions.Writer);
    private const string Allow = nameof(AuthorizationOptions.Allow);

    [SetUp]
    public void SetUp()
    {
        authorizationOptions = new AuthorizationOptions
        {
            SuperUser = SuperUser,
            SuperReader = SuperReader,
            Admin = Admin,
            Writer = Writer,
            Allow = Allow,
        };

        options.Value.Returns(authorizationOptions);

        identityService = new IdentityService(httpContextAccessorMock, options);
    }

    [TearDown]
    public void TearDown()
    {
        httpContextAccessorMock.ClearSubstitute();
    }

    [Test]
    public void GetEmailReturnsEmailClaim()
    {
        var httpContext = new DefaultHttpContext();
        const string expected = "unit@Test.ch";
        var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Email, expected) });
        httpContext.User.AddIdentity(identity);

        httpContextAccessorMock.HttpContext.Returns(httpContext);

        var actual = identityService.GetCurrentEmail();

        Assert.That(actual, Is.EqualTo(expected.ToLowerInvariant()));
    }

    [Test]
    public void GetEmailThrowsOnNoEmailClaim()
    {
        var httpContext = new DefaultHttpContext();

        httpContextAccessorMock.HttpContext.Returns(httpContext);

        Assert.That(() => identityService.GetCurrentEmail(), Throws.InstanceOf<InvalidOperationException>());
    }

    [Test]
    public void GetFirstNameReturnsGivenNameClaim()
    {
        var httpContext = new DefaultHttpContext();
        const string expected = "Hans";
        var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.GivenName, expected) });
        httpContext.User.AddIdentity(identity);

        httpContextAccessorMock.HttpContext.Returns(httpContext);

        var actual = identityService.GetCurrentFirstName();

        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public void GetFirstNameThrowsOnNoGivenNameClaim()
    {
        var httpContext = new DefaultHttpContext();

        httpContextAccessorMock.HttpContext.Returns(httpContext);

        Assert.That(() => identityService.GetCurrentFirstName(), Throws.InstanceOf<InvalidOperationException>());
    }

    [Test]
    public void GetLastNameReturnsSurnameClaim()
    {
        var httpContext = new DefaultHttpContext();
        const string expected = "Zimmermann";
        var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Surname, expected) });
        httpContext.User.AddIdentity(identity);

        httpContextAccessorMock.HttpContext.Returns(httpContext);

        var actual = identityService.GetCurrentLastName();

        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public void GetLastNameThrowsOnNoSurnameClaim()
    {
        var httpContext = new DefaultHttpContext();

        httpContextAccessorMock.HttpContext.Returns(httpContext);

        Assert.That(() => identityService.GetCurrentLastName(), Throws.InstanceOf<InvalidOperationException>());
    }

    [TestCase(new[] { Admin, Allow }, Admin)]
    [TestCase(new[] { Writer, Admin, Allow }, Admin)]
    [TestCase(new[] { Admin }, Admin)]
    [TestCase(new[] { SuperUser, Admin }, Admin)]
    [TestCase(new[] { SuperUser, Writer }, SuperUser)]
    [TestCase(new[] { SuperUser, Writer, Allow }, SuperUser)]
    [TestCase(new[] { SuperUser, Allow }, SuperUser)]
    [TestCase(new[] { SuperReader, Admin }, Admin)]
    [TestCase(new[] { SuperReader, Writer }, SuperReader)]
    [TestCase(new[] { SuperReader, Writer, Allow }, SuperReader)]
    [TestCase(new[] { SuperReader, Allow }, SuperReader)]
    [TestCase(new[] { Writer, Allow }, Writer)]
    [TestCase(new[] { Writer }, Writer)]
    [TestCase(new[] { Allow }, null)]
    [TestCase(new string[0], null)]
    [TestCase(new[] { "bla", "whatever" }, null)]
    public void GetCurrentRole_WithClaims_ReturnsRoleByHierarchy(IEnumerable<string> roles, string expectedRole)
    {
        var httpContext = new DefaultHttpContext();
        var identity = new ClaimsIdentity(roles.Select(r => new Claim(ClaimTypes.Role, r)));
        httpContext.User.AddIdentity(identity);

        httpContextAccessorMock.HttpContext.Returns(httpContext);

        var role = identityService.GetCurrentRole();

        Assert.That(role, Is.EqualTo(expectedRole));
    }

    [Test]
    public void GetCurrentRole_WithoutHttpContext_ReturnsNull()
    {
        httpContextAccessorMock.HttpContext.ReturnsNull();

        var role = identityService.GetCurrentRole();

        Assert.That(role, Is.Null);
    }
}
