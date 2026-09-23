using System.Collections.Generic;
using System.Security.Claims;
using AutoFixture;
using Bk.Termdat.Business.Domain.Models;
using Bk.Termdat.Business.Services;
using Bk.Termdat.CrossCutting;
using Bk.Termdat.CrossCutting.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using NSubstitute;
using NSubstitute.ClearExtensions;
using NUnit.Framework;

namespace Bk.Termdat.Business.Tests.Services;

[TestFixture]
public class AuthorizationServiceTests
{
    private AuthorizationService authorizationService;

    private readonly IOptions<AuthorizationOptions> authorizationOptionsMock = Substitute.For<IOptions<AuthorizationOptions>>();
    private readonly IHttpContextAccessor httpContextAccessorMock = Substitute.For<IHttpContextAccessor>();
    private readonly Fixture fixture = new();
    private readonly List<int> collectionIds = new() { 1, 2, 3, 15 };
    private readonly List<int> glossaryIds = new() { 4, 5, 6, 17 };

    private AuthorizationOptions authorizationOptions;
    private Entry entry;

    [SetUp]
    public void SetUp()
    {
        authorizationOptions = fixture.Create<AuthorizationOptions>();
        entry = new Entry
        {
            Id = 1
        };

        authorizationOptionsMock.Value.Returns(authorizationOptions);

        authorizationService = new AuthorizationService(httpContextAccessorMock, authorizationOptionsMock);

        var httpContext = new DefaultHttpContext();
        var identity = new ClaimsIdentity(new[]
        {
            new Claim(CustomClaimTypes.CollectionIds, string.Join(",", collectionIds)),
            new Claim(CustomClaimTypes.GlossaryIds, string.Join(",", glossaryIds))
        });

        httpContext.User.AddIdentity(identity);
        httpContextAccessorMock.HttpContext.Returns(httpContext);
    }

    [TearDown]
    public void TearDown()
    {
        authorizationOptionsMock.ClearSubstitute();
        httpContextAccessorMock.ClearSubstitute();
    }

    [Test]
    public void NoContextReturns()
    {
        httpContextAccessorMock.HttpContext.Returns((HttpContext)null);

        Assert.That(authorizationService.IsAdmin(), Is.False);
        Assert.That(authorizationService.IsSuperUser(), Is.False);
        Assert.That(authorizationService.IsSuperReader(), Is.False);
        Assert.That(authorizationService.IsWriter(), Is.False);
    }

    [Test]
    public void NoAuthenticationReturns()
    {
        var httpContext = new DefaultHttpContext();
        httpContextAccessorMock.HttpContext.Returns(httpContext);

        Assert.That(authorizationService.IsAdmin(), Is.False);
        Assert.That(authorizationService.IsSuperUser(), Is.False);
        Assert.That(authorizationService.IsSuperReader(), Is.False);
        Assert.That(authorizationService.IsWriter(), Is.False);
    }

    [Test]
    public void IsRoleReturnsWithAllRoleClaims()
    {
        var httpContext = new DefaultHttpContext();
        var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Role, authorizationOptions.Admin),
                new Claim(ClaimTypes.Role, authorizationOptions.SuperUser),
                new Claim(ClaimTypes.Role, authorizationOptions.SuperReader),
                new Claim(ClaimTypes.Role, authorizationOptions.Writer)
            });
        httpContext.User.AddIdentity(identity);
        httpContextAccessorMock.HttpContext.Returns(httpContext);

        Assert.That(authorizationService.IsAdmin(), Is.True);
        Assert.That(authorizationService.IsSuperUser(), Is.True);
        Assert.That(authorizationService.IsSuperReader(), Is.True);
        Assert.That(authorizationService.IsWriter(), Is.True);
    }

    [Test]
    public void IsAdminRoleReturnsWithAdminRoleClaim()
    {
        PrepareAdminContext();

        Assert.That(authorizationService.IsAdmin(), Is.True);
        Assert.That(authorizationService.IsSuperUser(), Is.False);
        Assert.That(authorizationService.IsSuperReader(), Is.False);
        Assert.That(authorizationService.IsWriter(), Is.False);
    }

    [Test]
    public void IsSuperUserRoleReturnsWithSuperUserRoleClaim()
    {
        PrepareSuperUserContext();

        Assert.That(authorizationService.IsAdmin(), Is.False);
        Assert.That(authorizationService.IsSuperUser(), Is.True);
        Assert.That(authorizationService.IsSuperReader(), Is.False);
        Assert.That(authorizationService.IsWriter(), Is.False);
    }

    [Test]
    public void IsSuperReaderRoleReturnsWithSuperUserRoleClaim()
    {
        PrepareSuperReaderContext();

        Assert.That(authorizationService.IsAdmin(), Is.False);
        Assert.That(authorizationService.IsSuperUser(), Is.False);
        Assert.That(authorizationService.IsSuperReader(), Is.True);
        Assert.That(authorizationService.IsWriter(), Is.False);
    }

    [Test]
    public void IsWriterRoleReturnsWithWriterRoleClaim()
    {
        PrepareWriterContext();

        Assert.That(authorizationService.IsAdmin(), Is.False);
        Assert.That(authorizationService.IsSuperUser(), Is.False);
        Assert.That(authorizationService.IsSuperReader(), Is.False);
        Assert.That(authorizationService.IsWriter(), Is.True);
    }

    [TestCase(4)]
    [TestCase(5)]
    [TestCase(6)]
    [TestCase(17)]
    public void HasAccessToGlossary_WithGlossaryPermission_ReturnsTrue(int glossaryId)
    {
        Assert.That(authorizationService.HasAccessToGlossary(glossaryId), Is.True);
    }

    [TestCase(1)]
    [TestCase(2)]
    [TestCase(3)]
    [TestCase(7)]
    public void HasAccessToGlossary_WithoutGlossaryPermission_ReturnsFalse(int glossaryId)
    {
        Assert.That(authorizationService.HasAccessToGlossary(glossaryId), Is.False);
    }

    [TestCase(1)]
    [TestCase(2)]
    [TestCase(3)]
    [TestCase(15)]
    public void HasAccessToCollection_WithCollectionPermission_ReturnsTrue(int collectionId)
    {
        Assert.That(authorizationService.HasAccessToCollection(collectionId), Is.True);
    }

    [Test]
    public void HasAccessToCollection_AsAdmin_ReturnsTrue()
    {
        var httpContext = new DefaultHttpContext();
        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Role, authorizationOptions.Admin),
        });
        httpContext.User.AddIdentity(identity);
        Assert.That(authorizationService.HasAccessToCollection(1), Is.True);
    }

    [Test]
    public void HasAccessToCollection_AsSuperUser_ReturnsTrue()
    {
        var httpContext = new DefaultHttpContext();
        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Role, authorizationOptions.SuperUser),
        });
        httpContext.User.AddIdentity(identity);
        Assert.That(authorizationService.HasAccessToCollection(1), Is.True);
    }

    [TestCase(4)]
    [TestCase(5)]
    [TestCase(6)]
    public void HasAccessToCollection_WithoutCollectionPermission_ReturnsFalse(int collectionId)
    {
        Assert.That(authorizationService.HasAccessToCollection(collectionId), Is.False);
    }

    [Test]
    public void GetAuthorizedGlossaryIds_WithoutClaim_ReturnsEmptyCollection()
    {
        PrepareAdminContext();
        Assert.That(authorizationService.GetAuthorizedGlossaryIds(), Is.Empty);
    }

    [Test]
    public void GetAuthorizedCollectionIds_WithoutClaim_ReturnsEmptyCollection()
    {
        PrepareAdminContext();
        Assert.That(authorizationService.GetAuthorizedCollectionIds(), Is.Empty);
    }

    [Test]
    public void GetAuthorizedGlossaryIds_WithClaim_ReturnsAuthorizedIds()
    {
        PrepareWriterContext();
        Assert.That(authorizationService.GetAuthorizedGlossaryIds(), Is.EquivalentTo(glossaryIds));
    }

    [Test]
    public void GetAuthorizedCollectionIds_WithClaim_ReturnsAuthorizeIds()
    {
        PrepareWriterContext();
        Assert.That(authorizationService.GetAuthorizedCollectionIds(), Is.EquivalentTo(collectionIds));
    }

    [Test]
    public void CanEdit_ForAdmin_ReturnsResult()
    {
        PrepareAdminContext();

        Assert.That(authorizationService.CanEdit(entry), Is.True);
    }

    [Test]
    public void CanEdit_ForSuperUser_ReturnsResult()
    {
        PrepareSuperUserContext();

        Assert.That(authorizationService.CanEdit(entry), Is.True);
    }

    [TestCase(1, 4, true)]
    [TestCase(1, 1, true)]
    [TestCase(4, 1, false)]
    public void CanEdit_ForWriter_ReturnsResult(int collectionId, int glossaryId, bool expected)
    {
        PrepareWriterContext();
        entry.Glossaries = new List<Glossary> { new() { Id = glossaryId } };
        entry.CollectionId = collectionId;

        Assert.That(authorizationService.CanEdit(entry), Is.EqualTo(expected));
    }

    public void CanAddToWatchList_ForAdmin_ReturnsTrue()
    {
        PrepareAdminContext();
        Assert.That(authorizationService.CanAddToWatchList(entry), Is.True);
    }

    public void CanAddToWatchList_ForSuperUser_ReturnsTrue()
    {
        PrepareSuperUserContext();

        Assert.That(authorizationService.CanAddToWatchList(entry), Is.True);
    }

    [TestCase(1, true)]
    [TestCase(4, false)]
    public void CanAddToWatchList_ForWriter_ReturnsResult(int collectionId, bool expected)
    {
        PrepareWriterContext();
        entry.CollectionId = collectionId;

        Assert.That(authorizationService.CanAddToWatchList(entry), Is.EqualTo(expected));
    }

    [TestCase(1, true)]
    [TestCase(4, false)]
    public void CanAddToWatchList_ForSuperReader_ReturnsExpected(int collectionId, bool expected)
    {
        PrepareSuperReaderContext();
        entry.CollectionId = collectionId;

        Assert.That(authorizationService.CanAddToWatchList(entry), Is.EqualTo(expected));
    }

    [Test]
    public void CanRestore_ForAdmin_ReturnsResult()
    {
        PrepareAdminContext();

        Assert.That(authorizationService.CanRestore(entry), Is.True);
    }

    [Test]
    public void CanRestore_ForSuperUser_ReturnsResult()
    {
        PrepareSuperUserContext();

        Assert.That(authorizationService.CanRestore(entry), Is.False);
    }

    [TestCase(1, 4, false)]
    [TestCase(1, 1, false)]
    [TestCase(4, 1, false)]
    public void CanRestore_ForWriter_ReturnsResult(int collectionId, int glossaryId, bool expected)
    {
        PrepareWriterContext();
        entry.Glossaries = new List<Glossary> { new() { Id = glossaryId } };
        entry.CollectionId = collectionId;

        Assert.That(authorizationService.CanRestore(entry), Is.EqualTo(expected));
    }

    [Test]
    public void CanDelete_ForAdmin_ReturnsResult()
    {
        PrepareAdminContext();

        Assert.That(authorizationService.CanDelete(entry), Is.True);
    }

    [Test]
    public void CanDelete_ForSuperUser_ReturnsResult()
    {
        PrepareSuperUserContext();

        Assert.That(authorizationService.CanDelete(entry), Is.True);
    }

    [TestCase(1, 4, false)]
    [TestCase(1, 1, false)]
    [TestCase(4, 1, false)]
    public void CanDelete_ForWriter_ReturnsResult(int collectionId, int glossaryId, bool expected)
    {
        PrepareWriterContext();
        entry.Glossaries = new List<Glossary> { new() { Id = glossaryId } };
        entry.CollectionId = collectionId;

        Assert.That(authorizationService.CanDelete(entry), Is.EqualTo(expected));
    }

    private void PrepareAdminContext()
    {
        var httpContext = new DefaultHttpContext();
        var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, authorizationOptions.Admin) });
        httpContext.User.AddIdentity(identity);
        httpContextAccessorMock.HttpContext.Returns(httpContext);
    }

    private void PrepareSuperUserContext()
    {
        var httpContext = new DefaultHttpContext();
        var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, authorizationOptions.SuperUser) });
        httpContext.User.AddIdentity(identity);
        httpContextAccessorMock.HttpContext.Returns(httpContext);
    }

    private void PrepareWriterContext()
    {
        var httpContext = new DefaultHttpContext();
        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Role, authorizationOptions.Writer),
            new Claim(CustomClaimTypes.CollectionIds, string.Join(",", collectionIds)),
            new Claim(CustomClaimTypes.GlossaryIds, string.Join(",", glossaryIds))
        });

        httpContext.User.AddIdentity(identity);
        httpContextAccessorMock.HttpContext.Returns(httpContext);
    }

    private void PrepareSuperReaderContext()
    {
        var httpContext = new DefaultHttpContext();
        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Role, authorizationOptions.SuperReader),
            new Claim(CustomClaimTypes.CollectionIds, string.Join(",", collectionIds)),
        });

        httpContext.User.AddIdentity(identity);
        httpContextAccessorMock.HttpContext.Returns(httpContext);
    }
}
