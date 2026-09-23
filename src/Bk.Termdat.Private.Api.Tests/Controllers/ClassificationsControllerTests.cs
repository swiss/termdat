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
internal class ClassificationsControllerTests
{
    private ClassificationsController classificationController;

    private readonly IClassificationService classificationServiceMock = Substitute.For<IClassificationService>();
    private IEnumerable<ClassificationSummary> classifications;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        var random = new Randomizer();
        var faker = new Faker<ClassificationSummary>().RuleFor(t => t.Name, _ => random.Word());
        classifications = Enumerable.Range(1, 8).Select(_ => faker.Generate());

        classificationServiceMock.GetAll().ReturnsForAnyArgs(classifications);
        classificationController = new ClassificationsController(classificationServiceMock);
    }

    [Test]
    public async Task GetAllShouldCallClassificationService()
    {
        var result = await classificationController.GetAll();

        await classificationServiceMock.Received().GetAll();

        Assert.That(result, Is.Not.Null);
        var response = result as OkObjectResult;
        Assert.That(response, Is.Not.Null);
        Assert.That(response.Value, Is.Not.Null);
        Assert.That(response.Value, Is.EqualTo(classifications));
        Assert.That(response.StatusCode, Is.EqualTo(200));
    }
}
