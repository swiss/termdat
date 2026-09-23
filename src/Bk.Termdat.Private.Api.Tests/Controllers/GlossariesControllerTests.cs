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

public class GlossariesControllerTests
{
    private GlossariesController glossaryController;

    private IGlossaryService glossaryServiceMock;
    private IEnumerable<GlossarySummary> glossarySummaries;
    private IEnumerable<GlossaryDetail> glossaryDetails;
    private GlossaryDetail glossaryDetail;
    private GlossaryCreate glossaryCreate;
    private GlossaryUpdate glossaryUpdate;

    [SetUp]
    public void SetUp()
    {
        var random = new Randomizer();
        var faker = new Faker<GlossarySummary>().RuleFor(t => t.Name, _ => random.Word());
        glossarySummaries = Enumerable.Range(1, 8).Select(_ => faker.Generate()).ToList();
        var fakerDetail = new Faker<GlossaryDetail>().RuleFor(t => t.Name, _ => random.Word());
        glossaryDetails = Enumerable.Range(1, 8).Select(_ => fakerDetail.Generate()).ToList();
        glossaryDetail = fakerDetail.Generate();
        glossaryCreate = new Faker<GlossaryCreate>().RuleFor(x => x.Code, f => f.Random.String(1, 10)).Generate();
        glossaryUpdate = new Faker<GlossaryUpdate>().RuleFor(x => x.Code, f => f.Random.String(1, 10)).Generate();

        glossaryServiceMock = Substitute.For<IGlossaryService>();
        glossaryServiceMock.GetAll().ReturnsForAnyArgs(glossarySummaries);
        glossaryServiceMock.GetAllDetails().ReturnsForAnyArgs(glossaryDetails);
        glossaryServiceMock.GetAllForModification().ReturnsForAnyArgs(glossaryDetails);
        glossaryServiceMock.GetDetail(0).ReturnsForAnyArgs(glossaryUpdate);
        glossaryController = new GlossariesController(glossaryServiceMock);
    }

    [Test]
    public async Task GetAllShouldCallGlossaryService()
    {
        var result = await glossaryController.GetAll();

        await glossaryServiceMock.Received().GetAll();

        Assert.That(result, Is.Not.Null);
        var response = result as OkObjectResult;
        Assert.That(response, Is.Not.Null);
        Assert.That(response.Value, Is.Not.Null);
        Assert.That(response.Value, Is.EqualTo(glossarySummaries));
        Assert.That(response.StatusCode, Is.EqualTo(200));
    }

    [Test]
    public async Task GetAllDetailsShouldCallGlossaryService()
    {
        var result = await glossaryController.GetAllDetails();

        await glossaryServiceMock.Received().GetAllDetails();

        Assert.That(result, Is.Not.Null);
        var response = result as OkObjectResult;
        Assert.That(response, Is.Not.Null);
        Assert.That(response.Value, Is.Not.Null);
        Assert.That(response.Value, Is.EqualTo(glossaryDetails));
        Assert.That(response.StatusCode, Is.EqualTo(200));
    }

    [Test]
    public async Task GetDetailShouldReturnGlossaryUpdate()
    {
        var result = await glossaryController.GetDetail(1);

        await glossaryServiceMock.Received().GetDetail(1);

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        var response = result as OkObjectResult;
        Assert.That(response, Is.Not.Null);
        Assert.That(response.Value, Is.Not.Null);
        Assert.That(response.Value, Is.EqualTo(glossaryUpdate));
        Assert.That(response.StatusCode, Is.EqualTo(200));
    }

    [Test]
    public async Task CreateShouldReturnBadRequest()
    {
        var result = await glossaryController.Create(null);

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<BadRequestResult>());
        var response = result as BadRequestResult;
        Assert.That(response, Is.Not.Null);
        Assert.That(response.StatusCode, Is.EqualTo(400));
    }

    [Test]
    public async Task CreateShouldCallGlossaryService()
    {
        glossaryServiceMock.Create(glossaryCreate).Returns(glossaryDetail);

        var result = await glossaryController.Create(glossaryCreate);
        await glossaryServiceMock.Received().Create(glossaryCreate);
        Assert.That(result, Is.Not.Null);

        var response = result as OkObjectResult;
        Assert.That(response, Is.Not.Null);
        Assert.That(response.Value, Is.EqualTo(glossaryDetail));
        Assert.That(response.StatusCode, Is.EqualTo(200));
    }

    [Test]
    public async Task UpdateShouldReturnBadRequestForEmptyForm()
    {
        var result = await glossaryController.Update(1, null);

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<BadRequestResult>());
        var response = result as BadRequestResult;
        Assert.That(response, Is.Not.Null);
        Assert.That(response.StatusCode, Is.EqualTo(400));
    }

    [Test]
    public async Task UpdateShouldReturnBadRequestForWrongId()
    {
        glossaryUpdate.Id = 2;
        var result = await glossaryController.Update(1, glossaryUpdate);

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<BadRequestResult>());
        var response = result as BadRequestResult;
        Assert.That(response, Is.Not.Null);
        Assert.That(response.StatusCode, Is.EqualTo(400));
    }

    [Test]
    public async Task UpdateShouldCallGlossaryService()
    {
        glossaryServiceMock.Update(glossaryUpdate).Returns(glossaryDetail);
        glossaryUpdate.Id = 1;
        var result = await glossaryController.Update(1, glossaryUpdate);

        await glossaryServiceMock.Received().Update(glossaryUpdate);
        Assert.That(result, Is.Not.Null);

        var response = result as OkObjectResult;
        Assert.That(response, Is.Not.Null);
        Assert.That(response.Value, Is.EqualTo(glossaryDetail));
        Assert.That(response.StatusCode, Is.EqualTo(200));
    }

    [Test]
    public async Task CanDelete()
    {
        var result = await glossaryController.Delete(1);

        await glossaryServiceMock.Received(1).Delete(Arg.Is(1));

        Assert.That(result, Is.Not.Null);
        var response = result as OkResult;
        Assert.That(response, Is.Not.Null);
    }

    [Test]
    public async Task GetGlossariesForModification_WhenCalled_ReturnsModificationGlossaries()
    {
        var result = await glossaryController.GetGlossariesForModification();

        await glossaryServiceMock.Received().GetAllForModification();

        Assert.That(result, Is.Not.Null);
        var response = result as OkObjectResult;
        Assert.That(response, Is.Not.Null);
        Assert.That(response.Value, Is.Not.Null);
        Assert.That(response.Value, Is.EqualTo(glossaryDetails));
        Assert.That(response.StatusCode, Is.EqualTo(200));
    }
}
