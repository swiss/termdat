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
internal class AdminInfosControllerTests
{
    private AdminInfosController adminInfoController;

    private readonly IAdminInfoService adminInfoServiceMock = Substitute.For<IAdminInfoService>();
    private IEnumerable<AdminInfoSummary> adminInfos;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        var faker = new Faker<AdminInfoSummary>();

        adminInfos = Enumerable.Range(1, 8).Select(_ => faker.Generate());

        adminInfoServiceMock.GetAll().ReturnsForAnyArgs(adminInfos);
        adminInfoController = new AdminInfosController(adminInfoServiceMock);
    }

    [Test]
    public async Task GetAllShouldCallAdminInfoService()
    {
        var result = await adminInfoController.GetAll();

        await adminInfoServiceMock.Received().GetAll();

        Assert.That(result, Is.Not.Null);
        var response = result as OkObjectResult;
        Assert.That(response, Is.Not.Null);
        Assert.That(response.Value, Is.Not.Null);
        Assert.That(response.StatusCode, Is.EqualTo(200));
    }
}
