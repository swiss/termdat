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
internal class LenochClassificationsControllerTests
{
    private LenochClassificationsController lenochClassificationController;

    private readonly ILenochClassificationService lenochClassificationServiceMock = Substitute.For<ILenochClassificationService>();
    private IEnumerable<LenochClassificationSummary> lenochClassifications;
    private IEnumerable<LenochClassificationDetail> lenochClassificationDetails;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        var random = new Randomizer();
        var faker = new Faker<LenochClassificationSummary>().RuleFor(t => t.Name, _ => random.Word());
        lenochClassifications = Enumerable.Range(1, 8).Select(_ => faker.Generate());
        var detailFaker = new Faker<LenochClassificationDetail>().RuleFor(t => t.Name, _ => random.Word());
        lenochClassificationDetails = Enumerable.Range(1, 8).Select(_ => detailFaker.Generate());

        lenochClassificationServiceMock.GetAll().Returns(lenochClassifications);
        lenochClassificationServiceMock.GetAllDetails().Returns(lenochClassificationDetails);
        lenochClassificationController = new LenochClassificationsController(lenochClassificationServiceMock);
    }

    [Test]
    public async Task ShouldGetAll()
    {
        var result = await lenochClassificationController.GetAll();

        await lenochClassificationServiceMock.Received(1).GetAll();

        Assert.That(result, Is.Not.Null);
        var response = result as OkObjectResult;
        Assert.That(response, Is.Not.Null);
        Assert.That(response.Value, Is.Not.Null);
        Assert.That(response.Value, Is.EqualTo(lenochClassifications));
        Assert.That(response.StatusCode, Is.EqualTo(200));
    }

    [Test]
    public async Task ShouldGetAllDetails()
    {
        var result = await lenochClassificationController.GetAllDetails();

        await lenochClassificationServiceMock.Received(1).GetAllDetails();

        Assert.That(result, Is.Not.Null);
        var response = result as OkObjectResult;
        Assert.That(response, Is.Not.Null);
        Assert.That(response.Value, Is.Not.Null);
        Assert.That(response.Value, Is.EqualTo(lenochClassificationDetails));
        Assert.That(response.StatusCode, Is.EqualTo(200));
    }
}
