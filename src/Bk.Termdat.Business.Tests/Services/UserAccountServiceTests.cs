using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using AutoMapper;
using Bk.Termdat.Business.Domain;
using Bk.Termdat.Business.Domain.Models;
using Bk.Termdat.Business.Repositories;
using Bk.Termdat.Business.Resource.Models;
using Bk.Termdat.Business.Services;
using Bk.Termdat.Business.Services.Interfaces;
using Bk.Termdat.CrossCutting.Configuration;
using Bk.Termdat.CrossCutting.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using NUnit.Framework;

namespace Bk.Termdat.Business.Tests.Services;

[TestFixture]
internal class UserAccountServiceTests
{
    private UserAccountService userAccountService;
    private IMapper mapperMock;
    private IUserAccountRepository userAccountRepositoryMock;
    private IUserAccountValidationService userAccountValidationService;
    private IIdentityService identityServiceMock;
    private IUnitOfWork unitOfWorkMock;
    private readonly Fixture fixture = new();
    private IOptions<AuthorizationOptions> authorizationOptions;

    private const string SuperUser = nameof(AuthorizationOptions.SuperUser);
    private const string SuperReader = nameof(AuthorizationOptions.SuperReader);
    private const string Admin = nameof(AuthorizationOptions.Admin);
    private const string Writer = nameof(AuthorizationOptions.Writer);
    private const string Allow = nameof(AuthorizationOptions.Allow);

    private readonly ICollection<Collection> currentCollections = new List<Collection> { new() { Id = 101, Code = "C01" }, new() { Id = 102, Code = "C02" } };
    private readonly ICollection<Glossary> currentGlossaries = new List<Glossary> { new() { Id = 201, Code = "G01" }, new() { Id = 202, Code = "G02" } };

    private readonly ICollection<Collection> newCollections = new List<Collection> { new() { Id = 301, Code = "X01" }, new() { Id = 302, Code = "X02" } };
    private readonly ICollection<Glossary> newGlossaries = new List<Glossary> { new() { Id = 401, Code = "Y01" } };

    private UserAccountModification userAccountModification;

    [SetUp]
    public void SetUp()
    {
        authorizationOptions = Options.Create(new AuthorizationOptions
        {
            SuperUser = SuperUser,
            SuperReader = SuperReader,
            Admin = Admin,
            Writer = Writer,
            Allow = Allow,
        });

        mapperMock = Substitute.For<IMapper>();
        userAccountRepositoryMock = Substitute.For<IUserAccountRepository>();
        identityServiceMock = Substitute.For<IIdentityService>();
        userAccountValidationService = Substitute.For<IUserAccountValidationService>();
        unitOfWorkMock = Substitute.For<IUnitOfWork>();
        unitOfWorkMock = Substitute.For<IUnitOfWork>();
        userAccountService = new UserAccountService(mapperMock, userAccountRepositoryMock, identityServiceMock, unitOfWorkMock, Substitute.For<ILogger<UserAccountService>>(), userAccountValidationService, authorizationOptions);

        userAccountModification = new UserAccountModification
        {
            Id = 1,
            AuthorizedCollectionIds = newCollections.Select(y => y.Id).ToArray(),
            AuthorizedGlossaryIds = newGlossaries.Select(y => y.Id).ToArray(),
        };
    }

    [Test]
    public async Task GetAll_WhenCalled_CallsRepository()
    {
        var userAccounts = new List<UserAccount> { new(), new() };
        userAccountRepositoryMock.GetAll().Returns(userAccounts);
        var userAccountSummaries = fixture.CreateMany<UserAccountSummary>();
        mapperMock.Map<IEnumerable<UserAccountSummary>>(userAccounts).Returns(userAccountSummaries);

        var result = await userAccountService.GetAll();

        Assert.That(result, Is.EqualTo(userAccountSummaries));
        await userAccountRepositoryMock.Received().GetAll();
    }

    [Test]
    public async Task CreateOrUpdate_ForFirstLogin_CreatesNewAccount()
    {
        var currentEmail = fixture.Create<string>();
        var currentLastName = fixture.Create<string>();
        var currentFirstName = fixture.Create<string>();
        var currentRole = fixture.Create<string>();

        identityServiceMock.GetCurrentEmail().Returns(currentEmail);
        identityServiceMock.GetCurrentFirstName().Returns(currentFirstName);
        identityServiceMock.GetCurrentLastName().Returns(currentLastName);
        identityServiceMock.GetCurrentRole().Returns(currentRole);

        UserAccount receivedUserAccount = null!;
        userAccountRepositoryMock.When(x => x.Create(Arg.Any<UserAccount>()))
            .Do(x => receivedUserAccount = x.Arg<UserAccount>());

        await userAccountService.CreateOrUpdate();

        await userAccountRepositoryMock.Received(1).Create(receivedUserAccount);
        await unitOfWorkMock.Received(1).SaveChangesAsync();
        await userAccountRepositoryMock.Received(1).GetDetailByEmail(Arg.Any<string>());
        await userAccountRepositoryMock.Received(1).GetDetailByEmailForUpdate(Arg.Any<string>());
        mapperMock.Received(1).Map<UserAccountDetail>(Arg.Any<UserAccount>());

        Assert.That(receivedUserAccount.Email, Is.EqualTo(currentEmail));
        Assert.That(receivedUserAccount.FirstName, Is.EqualTo(currentFirstName));
        Assert.That(receivedUserAccount.LastName, Is.EqualTo(currentLastName));
        Assert.That(receivedUserAccount.Role, Is.EqualTo(currentRole));
        Assert.That(receivedUserAccount.LastLogin, Is.GreaterThanOrEqualTo(DateTime.UtcNow.AddMinutes(-1)));
        Assert.That(receivedUserAccount.Created, Is.GreaterThanOrEqualTo(DateTime.UtcNow.AddMinutes(-1)));
        Assert.That(receivedUserAccount.CreatedBy, Is.EqualTo("System"));
        Assert.That(receivedUserAccount.Modified, Is.GreaterThanOrEqualTo(DateTime.UtcNow.AddMinutes(-1)));
        Assert.That(receivedUserAccount.ModifiedBy, Is.EqualTo("System"));
    }

    [Test]
    public async Task CreateOrUpdate_ForExistingAccount_UpdatesExistingAccount()
    {
        var currentEmail = fixture.Create<string>();
        var currentLastName = fixture.Create<string>();
        var currentFirstName = fixture.Create<string>();
        var currentRole = fixture.Create<string>();
        identityServiceMock.GetCurrentEmail().Returns(currentEmail);
        identityServiceMock.GetCurrentFirstName().Returns(currentFirstName);
        identityServiceMock.GetCurrentLastName().Returns(currentLastName);
        identityServiceMock.GetCurrentRole().Returns(currentRole);

        var userAccount = new UserAccount
        {
            Id = 1,
            Created = DateTime.UtcNow,
            CreatedBy = nameof(UserAccount.CreatedBy),
            Modified = DateTime.UtcNow,
            ModifiedBy = nameof(UserAccount.ModifiedBy),
            LastLogin = DateTime.UtcNow.AddDays(-7),
            LastAction = DateTime.UtcNow.AddDays(-5),
            Role = "old role",
            FirstName = "old FirstName",
            LastName = "old LastName",
        };
        userAccountRepositoryMock.GetDetailByEmail(currentEmail).Returns(userAccount);
        userAccountRepositoryMock.GetDetailByEmailForUpdate(currentEmail).Returns(userAccount);

        UserAccount receivedUserAccount = null!;
        userAccountRepositoryMock.When(x => x.Update(Arg.Any<UserAccount>(), Arg.Any<UserAccount>()))
            .Do(x => receivedUserAccount = x.ArgAt<UserAccount>(1));

        await userAccountService.CreateOrUpdate();

        userAccountRepositoryMock.Received(1).Update(userAccount, Arg.Any<UserAccount>());
        await unitOfWorkMock.Received().SaveChangesAsync();
        await userAccountRepositoryMock.Received(1).GetDetailByEmailForUpdate(currentEmail);
        await userAccountRepositoryMock.Received(1).GetDetailByEmail(currentEmail);
        mapperMock.Received(1).Map<UserAccountDetail>(userAccount);

        Assert.That(receivedUserAccount.Id, Is.EqualTo(userAccount.Id));
        Assert.That(receivedUserAccount.Created, Is.EqualTo(userAccount.Created));
        Assert.That(receivedUserAccount.CreatedBy, Is.EqualTo(userAccount.CreatedBy));
        Assert.That(receivedUserAccount.Modified, Is.GreaterThan(userAccount.Modified));
        Assert.That(receivedUserAccount.ModifiedBy, Is.EqualTo("System"));
        Assert.That(receivedUserAccount.Email, Is.EqualTo(currentEmail));
        Assert.That(receivedUserAccount.LastName, Is.EqualTo(currentLastName));
        Assert.That(receivedUserAccount.FirstName, Is.EqualTo(currentFirstName));
        Assert.That(receivedUserAccount.Role, Is.EqualTo(currentRole));
        Assert.That(receivedUserAccount.LastAction, Is.EqualTo(userAccount.LastAction));
        Assert.That(receivedUserAccount.LastLogin, Is.GreaterThan(userAccount.LastLogin));
    }

    [Test]
    public async Task GetAllDetails_WhenCalled_ReturnsDetails()
    {
        var userAccounts = new List<UserAccount> { new(), new() };
        userAccountRepositoryMock.GetAllDetailed().Returns(userAccounts);
        var detailedUserAccounts = fixture.CreateMany<UserAccountDetail>();
        mapperMock.Map<IEnumerable<UserAccountDetail>>(userAccounts).Returns(detailedUserAccounts);

        var result = await userAccountService.GetAllDetails();

        Assert.That(result, Is.EqualTo(detailedUserAccounts));
        await userAccountRepositoryMock.Received(1).GetAllDetailed();
    }

    [Test]
    public async Task UpdateLastAction_WithValidEmail_UpdatesLastAction()
    {
        const string email = "test@test.ch";
        var user = new UserAccount { Id = 1, };

        userAccountRepositoryMock.GetByEmail(email).Returns(user);

        await userAccountService.UpdateLastAction(email);

        Assert.That(user.LastAction, Is.GreaterThanOrEqualTo(DateTime.UtcNow.AddSeconds(-10)));
    }

    [Test]
    public void UpdateLastAction_WithInvalidEmail_ThrowsEntityNotFoundException()
    {
        const string email = "test@test.ch";
        userAccountRepositoryMock.GetByEmail(email).ReturnsNull();

        Assert.ThrowsAsync<EntityNotFoundException>(async () => await userAccountService.UpdateLastAction(email));
    }

    [Test]
    public async Task GetDetail_WhenCalled_ReturnsDetails()
    {
        var userAccount = new UserAccount();
        userAccountRepositoryMock.GetDetail(1).Returns(userAccount);
        var detailedUserAccount = new UserAccountDetail();
        mapperMock.Map<UserAccountDetail>(userAccount).Returns(detailedUserAccount);

        var result = await userAccountService.GetDetail(1);

        Assert.That(result, Is.EqualTo(detailedUserAccount));
        await userAccountRepositoryMock.Received(1).GetDetail(1);
    }

    public async Task Update_WithValidWriterUser_Success()
    {
        var currentEmail = fixture.Create<string>();
        identityServiceMock.GetCurrentEmail().Returns(currentEmail);

        var dbUserAccount = new UserAccount()
        {
            Id = 1,
            FirstName = "Franziska",
            LastName = "Testerin",
            Email = "franziska.testerin@test.ch",
            Role = "Writer",
            Created = DateTime.UtcNow.AddDays(-10),
            CreatedBy = "test1@test.ch",
            Modified = DateTime.UtcNow.AddDays(-5),
            ModifiedBy = "test2@test.ch",
            AuthorizedCollections = currentCollections,
            AuthorizedGlossaries = currentGlossaries
        };

        var mappedUserAccount = new UserAccount()
        {
            Id = 1,
            AuthorizedCollections = newCollections,
            AuthorizedGlossaries = newGlossaries
        };

        userAccountRepositoryMock.GetDetailByIdForUpdate(1).Returns(dbUserAccount);
        mapperMock.Map<UserAccount>(userAccountModification).Returns(mappedUserAccount);
        userAccountValidationService.ValidateCollections(userAccountModification).Returns(newCollections.ToList());
        userAccountValidationService.ValidateGlossaries(userAccountModification).Returns(newGlossaries.ToList());

        await userAccountService.Update(userAccountModification);

        await userAccountRepositoryMock.Received(1).GetDetailByIdForUpdate(1);
        userAccountRepositoryMock.Received(1).UpdateWithRelations(dbUserAccount, mappedUserAccount);
        await unitOfWorkMock.Received().SaveChangesAsync();
        Assert.That(mappedUserAccount.AuthorizedCollections, Is.Not.Null);
        Assert.That(mappedUserAccount.AuthorizedGlossaries, Is.Not.Null);
        Assert.That(mappedUserAccount.AuthorizedCollections.Count, Is.EqualTo(2));
        Assert.That(mappedUserAccount.AuthorizedGlossaries.Count, Is.EqualTo(1));
        Assert.That(mappedUserAccount.AuthorizedCollections.FirstOrDefault(y => y.Id == 301), Is.Not.Null);
        Assert.That(mappedUserAccount.AuthorizedCollections.FirstOrDefault(y => y.Id == 302), Is.Not.Null);
        Assert.That(mappedUserAccount.AuthorizedGlossaries.FirstOrDefault(y => y.Id == 401), Is.Not.Null);
        Assert.That(mappedUserAccount.Email, Is.EqualTo(dbUserAccount.Email));
        Assert.That(mappedUserAccount.FirstName, Is.EqualTo(dbUserAccount.FirstName));
        Assert.That(mappedUserAccount.LastName, Is.EqualTo(dbUserAccount.LastName));
        Assert.That(mappedUserAccount.Role, Is.EqualTo(dbUserAccount.Role));
        Assert.That(mappedUserAccount.LastLogin, Is.EqualTo(dbUserAccount.LastLogin));
        Assert.That(mappedUserAccount.LastLogin, Is.EqualTo(dbUserAccount.LastAction));
        Assert.That(mappedUserAccount.Created, Is.EqualTo(dbUserAccount.Created));
        Assert.That(mappedUserAccount.CreatedBy, Is.EqualTo(dbUserAccount.CreatedBy));
        Assert.That(mappedUserAccount.Modified, Is.GreaterThanOrEqualTo(DateTime.UtcNow.AddMinutes(-1)));
        Assert.That(mappedUserAccount.ModifiedBy, Is.EqualTo(currentEmail));
    }

    public async Task Update_WithValidSuperReaderUser_Success()
    {
        var currentEmail = fixture.Create<string>();
        identityServiceMock.GetCurrentEmail().Returns(currentEmail);

        var dbUserAccount = new UserAccount()
        {
            Id = 1,
            FirstName = "Franziska",
            LastName = "Testerin",
            Email = "franziska.testerin@test.ch",
            Role = "SuperReader",
            Created = DateTime.UtcNow.AddDays(-10),
            CreatedBy = "test1@test.ch",
            Modified = DateTime.UtcNow.AddDays(-5),
            ModifiedBy = "test2@test.ch",
            AuthorizedCollections = currentCollections,
            AuthorizedGlossaries = currentGlossaries
        };

        var mappedUserAccount = new UserAccount()
        {
            Id = 1,
            AuthorizedCollections = newCollections,
            AuthorizedGlossaries = newGlossaries
        };

        userAccountRepositoryMock.GetDetailByIdForUpdate(1).Returns(dbUserAccount);
        mapperMock.Map<UserAccount>(userAccountModification).Returns(mappedUserAccount);
        userAccountValidationService.ValidateCollections(userAccountModification).Returns(newCollections.ToList());
        userAccountValidationService.ValidateGlossaries(userAccountModification).Returns(newGlossaries.ToList());

        await userAccountService.Update(userAccountModification);

        await userAccountRepositoryMock.Received(1).GetDetailByIdForUpdate(1);
        userAccountRepositoryMock.Received(1).UpdateWithRelations(dbUserAccount, mappedUserAccount);
        await unitOfWorkMock.Received().SaveChangesAsync();
        Assert.That(mappedUserAccount.AuthorizedCollections, Is.Not.Null);
        Assert.That(mappedUserAccount.AuthorizedGlossaries, Is.Null);
        Assert.That(mappedUserAccount.AuthorizedCollections.Count, Is.EqualTo(2));
        Assert.That(mappedUserAccount.AuthorizedCollections.FirstOrDefault(y => y.Id == 301), Is.Not.Null);
        Assert.That(mappedUserAccount.AuthorizedCollections.FirstOrDefault(y => y.Id == 302), Is.Not.Null);
        Assert.That(mappedUserAccount.AuthorizedGlossaries.FirstOrDefault(y => y.Id == 401), Is.Not.Null);
        Assert.That(mappedUserAccount.Email, Is.EqualTo(dbUserAccount.Email));
        Assert.That(mappedUserAccount.FirstName, Is.EqualTo(dbUserAccount.FirstName));
        Assert.That(mappedUserAccount.LastName, Is.EqualTo(dbUserAccount.LastName));
        Assert.That(mappedUserAccount.Role, Is.EqualTo(dbUserAccount.Role));
        Assert.That(mappedUserAccount.LastLogin, Is.EqualTo(dbUserAccount.LastLogin));
        Assert.That(mappedUserAccount.LastLogin, Is.EqualTo(dbUserAccount.LastAction));
        Assert.That(mappedUserAccount.Created, Is.EqualTo(dbUserAccount.Created));
        Assert.That(mappedUserAccount.CreatedBy, Is.EqualTo(dbUserAccount.CreatedBy));
        Assert.That(mappedUserAccount.Modified, Is.GreaterThanOrEqualTo(DateTime.UtcNow.AddMinutes(-1)));
        Assert.That(mappedUserAccount.ModifiedBy, Is.EqualTo(currentEmail));
    }

    [Test]
    public void Update_WithNotWriterOrSuperReaderUser_ThrowsBusinessValidationException()
    {
        var currentEmail = fixture.Create<string>();
        identityServiceMock.GetCurrentEmail().Returns(currentEmail);

        var dbUserAccount = new UserAccount()
        {
            Id = 1,
            Role = "Admin",
            AuthorizedCollections = currentCollections,
            AuthorizedGlossaries = currentGlossaries
        };

        var mappedUserAccount = new UserAccount()
        {
            Id = 1,
            AuthorizedCollections = new List<Collection> { new() { Id = 301, Code = "X01" }, new() { Id = 302, Code = "X02" } },
            AuthorizedGlossaries = new List<Glossary> { new() { Id = 401, Code = "Y01" } }
        };

        userAccountRepositoryMock.GetDetailByIdForUpdate(1).Returns(dbUserAccount);
        mapperMock.Map<UserAccount>(userAccountModification).Returns(mappedUserAccount);
        userAccountValidationService.ValidateCollections(userAccountModification).Returns(newCollections.ToList());
        userAccountValidationService.ValidateGlossaries(userAccountModification).Returns(newGlossaries.ToList());

        Assert.ThrowsAsync<BusinessValidationException>(async () => await userAccountService.Update(userAccountModification));
    }

    [Test]
    public void Update_WithNotExistingUser_ThrowsEntityNotFoundException()
    {
        var currentEmail = fixture.Create<string>();
        identityServiceMock.GetCurrentEmail().Returns(currentEmail);

        userAccountModification.Id = 10;

        var dbUserAccount = new UserAccount()
        {
            Id = 1,
            Role = "Admin",
            AuthorizedCollections = currentCollections,
            AuthorizedGlossaries = currentGlossaries
        };

        var mappedUserAccount = new UserAccount()
        {
            Id = 1,
            AuthorizedCollections = newCollections,
            AuthorizedGlossaries = newGlossaries
        };

        userAccountRepositoryMock.GetDetailByIdForUpdate(1).Returns(dbUserAccount);
        mapperMock.Map<UserAccount>(userAccountModification).Returns(mappedUserAccount);
        userAccountValidationService.ValidateCollections(userAccountModification).Returns(newCollections.ToList());
        userAccountValidationService.ValidateGlossaries(userAccountModification).Returns(newGlossaries.ToList());

        Assert.ThrowsAsync<EntityNotFoundException>(async () => await userAccountService.Update(userAccountModification));
    }
}
