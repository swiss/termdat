using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bk.Termdat.Business.Resource.Models;
using Bk.Termdat.Business.Services.Interfaces;
using Bk.Termdat.Private.Api.Controllers;
using Bogus;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NUnit.Framework;

namespace Bk.Termdat.Private.Api.Tests.Controllers;

[TestFixture]
internal class UserAccountsControllerTests
{
    private UserAccountsController userAccountController;

    private readonly IUserAccountService userAccountServiceMock = Substitute.For<IUserAccountService>();
    private IEnumerable<UserAccountSummary> userAccounts;
    private IEnumerable<UserAccountDetail> detailedUserAccounts;
    private UserAccountDetail detailedUserAccount;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        var summaryFaker = new Faker<UserAccountSummary>()
            .RuleFor(t => t.UserName, f => f.Random.Word());
        var detailFaker = new Faker<UserAccountDetail>()
            .RuleFor(t => t.Email, f => f.Internet.Email());
        userAccounts = Enumerable.Range(1, 8).Select(_ => summaryFaker.Generate());
        detailedUserAccount = detailFaker.Generate();
        detailedUserAccounts = Enumerable.Range(1, 8).Select(_ => detailFaker.Generate());

        userAccountServiceMock.GetAll().ReturnsForAnyArgs(userAccounts);
        userAccountServiceMock.GetAllDetails().ReturnsForAnyArgs(detailedUserAccounts);
        userAccountServiceMock.CreateOrUpdate().ReturnsForAnyArgs(detailedUserAccount);

        userAccountController = new UserAccountsController(userAccountServiceMock);
    }

    [Test]
    public async Task GetAll_WhenCalled_ReturnsUserAccounts()
    {
        var result = await userAccountController.GetAll();

        await userAccountServiceMock.Received(1).GetAll();

        Assert.That(result, Is.Not.Null);

        var response = result as OkObjectResult;
        Assert.That(response, Is.Not.Null);
        Assert.That(response.Value, Is.Not.Null);
        Assert.That(response.Value, Is.EqualTo(userAccounts));
        Assert.That(response.StatusCode, Is.EqualTo(200));
    }

    [Test]
    public async Task CreateOrUpdate_WhenCalled_ReturnsAccountDetails()
    {
        var result = await userAccountController.CreateOrUpdate();

        await userAccountServiceMock.Received(1).CreateOrUpdate();

        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        var response = result as OkObjectResult;
        Assert.That(response, Is.Not.Null);
        Assert.That(response.Value, Is.Not.Null);
        Assert.That(response.Value, Is.EqualTo(detailedUserAccount));
        Assert.That(response.StatusCode, Is.EqualTo(200));
    }

    [Test]
    public async Task Update_WhenCalled_CallsService()
    {
        var dto = new UserAccountModification();
        var result = await userAccountController.Update(dto);

        await userAccountServiceMock.Received(1).Update(dto);

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<OkResult>());
    }

    [Test]
    public async Task GetAllDetails_WhenCalled_ReturnsDetailedUserAccounts()
    {
        var result = await userAccountController.GetAllDetails();

        await userAccountServiceMock.Received(1).GetAllDetails();

        Assert.That(result, Is.Not.Null);

        var response = result as OkObjectResult;
        Assert.That(response, Is.Not.Null);
        Assert.That(response.Value, Is.Not.Null);
        Assert.That(response.Value, Is.EqualTo(detailedUserAccounts));
        Assert.That(response.StatusCode, Is.EqualTo(200));
    }
}
