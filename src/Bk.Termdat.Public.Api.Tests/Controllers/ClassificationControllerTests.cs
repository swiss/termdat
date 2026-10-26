using System.Threading.Tasks;
using AutoFixture;
using Bk.Termdat.Business.Public.Api.Resource.Models;
using Bk.Termdat.Business.Public.Api.Services;
using Bk.Termdat.Public.Api.Controllers;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NUnit.Framework;

namespace Bk.Termdat.Public.Api.Tests.Controllers;

[TestFixture]
internal class ClassificationControllerTests
{
    private ClassificationController classificationController;
    private IClassificationService classificationServiceMock;

    private readonly Fixture fixture = new();

    [SetUp]
    public void Setup()
    {
        classificationServiceMock = Substitute.For<IClassificationService>();

        classificationController = new ClassificationController(classificationServiceMock);
    }

    [Test]
    public async Task GetClassificationsReturnsOk()
    {
        var languageCode = fixture.Create<string>();
        var classificationSummaries = fixture.CreateMany<ClassificationSummary>();
        classificationServiceMock.GetAll(languageCode).Returns(classificationSummaries);

        var result = await classificationController.GetClassifications(languageCode);

        await classificationServiceMock.Received(1).GetAll(languageCode);
        Assert.That(result, Is.Not.Null);
        var objectOkResult = result as OkObjectResult;

        Assert.That(objectOkResult, Is.Not.Null);
        Assert.That(objectOkResult.Value, Is.EqualTo(classificationSummaries));
    }
}
