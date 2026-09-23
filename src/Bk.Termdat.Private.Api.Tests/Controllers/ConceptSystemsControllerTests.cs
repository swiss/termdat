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
internal class ConceptSystemsControllerTests
{
    private readonly IConceptSystemService conceptSystemServiceMock = Substitute.For<IConceptSystemService>();
    private IEnumerable<ConceptSystemSummary> conceptSystems;
    private IEnumerable<ConceptSystemDetail> conceptSystemDetails;
    private ConceptSystemsController conceptSystemController;
    private ConceptSystemDetail conceptSystemDetail;
    private ConceptSystemCreate conceptSystemCreate;
    private ConceptSystemUpdate conceptSystemUpdate;
    [SetUp]
    public void Setup()
    {
        var random = new Randomizer();
        conceptSystems = Enumerable.Range(1, 8).Select(_ => new Faker<ConceptSystemSummary>().Generate());
        conceptSystemDetails = Enumerable.Range(1, 8).Select(_ => new Faker<ConceptSystemDetail>().Generate());
        var fakerDetail = new Faker<ConceptSystemDetail>().RuleFor(t => t.Title, _ => random.Word());
        conceptSystemDetail = fakerDetail.Generate();
        conceptSystemCreate = new Faker<ConceptSystemCreate>().RuleFor(x => x.Title, f => f.Random.String(1, 10)).Generate();
        conceptSystemUpdate = new Faker<ConceptSystemUpdate>().RuleFor(x => x.Title, f => f.Random.String(1, 10)).Generate();

        conceptSystemServiceMock.GetAll().ReturnsForAnyArgs(conceptSystems);
        conceptSystemServiceMock.GetAllDetails().ReturnsForAnyArgs(conceptSystemDetails);
        conceptSystemServiceMock.GetDetail(0).ReturnsForAnyArgs(conceptSystemUpdate);

        conceptSystemController = new ConceptSystemsController(conceptSystemServiceMock);
    }

    [Test]
    public async Task GetAllShouldCallService()
    {
        var result = await conceptSystemController.GetAll();

        await conceptSystemServiceMock.Received().GetAll();

        Assert.That(result, Is.Not.Null);
        var response = result as OkObjectResult;
        Assert.That(response, Is.Not.Null);
        Assert.That(response.Value, Is.Not.Null);
        Assert.That(response.Value, Is.EqualTo(conceptSystems));
        Assert.That(response.StatusCode, Is.EqualTo(200));
    }

    [Test]
    public async Task GetAllDetailsShouldCallService()
    {
        var result = await conceptSystemController.GetAllDetails();

        await conceptSystemServiceMock.Received().GetAllDetails();

        Assert.That(result, Is.Not.Null);
        var response = result as OkObjectResult;
        Assert.That(response, Is.Not.Null);
        Assert.That(response.Value, Is.Not.Null);
        Assert.That(response.Value, Is.EqualTo(conceptSystemDetails));
        Assert.That(response.StatusCode, Is.EqualTo(200));
    }

    [Test]
    public async Task GetDetailShouldReturnCollectionUpdate()
    {
        var result = await conceptSystemController.GetDetail(1);

        await conceptSystemServiceMock.Received().GetDetail(1);

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        var response = result as OkObjectResult;
        Assert.That(response, Is.Not.Null);
        Assert.That(response.Value, Is.Not.Null);
        Assert.That(response.Value, Is.EqualTo(conceptSystemUpdate));
        Assert.That(response.StatusCode, Is.EqualTo(200));
    }

    [Test]
    public async Task CreateShouldReturnBadRequest()
    {
        var result = await conceptSystemController.Create(null);

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<BadRequestResult>());
        var response = result as BadRequestResult;
        Assert.That(response, Is.Not.Null);
        Assert.That(response.StatusCode, Is.EqualTo(400));
    }

    [Test]
    public async Task CreateShouldCallConceptSystemService()
    {
        conceptSystemServiceMock.Create(conceptSystemCreate).Returns(conceptSystemDetail);

        var result = await conceptSystemController.Create(conceptSystemCreate);
        await conceptSystemServiceMock.Received().Create(conceptSystemCreate);
        Assert.That(result, Is.Not.Null);

        var response = result as OkObjectResult;
        Assert.That(response, Is.Not.Null);
        Assert.That(response.Value, Is.EqualTo(conceptSystemDetail));
        Assert.That(response.StatusCode, Is.EqualTo(200));
    }

    [Test]
    public async Task UpdateShouldReturnBadRequestForEmptyForm()
    {
        var result = await conceptSystemController.Update(1, null);

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<BadRequestResult>());
        var response = result as BadRequestResult;
        Assert.That(response, Is.Not.Null);
        Assert.That(response.StatusCode, Is.EqualTo(400));
    }

    [Test]
    public async Task UpdateShouldReturnBadRequestForWrongId()
    {
        conceptSystemUpdate.Id = 2;
        var result = await conceptSystemController.Update(1, conceptSystemUpdate);

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<BadRequestResult>());
        var response = result as BadRequestResult;
        Assert.That(response, Is.Not.Null);
        Assert.That(response.StatusCode, Is.EqualTo(400));
    }

    [Test]
    public async Task UpdateShouldCallConceptSystemService()
    {
        conceptSystemServiceMock.Update(conceptSystemUpdate).Returns(conceptSystemDetail);
        conceptSystemUpdate.Id = 1;
        var result = await conceptSystemController.Update(1, conceptSystemUpdate);

        await conceptSystemServiceMock.Received().Update(conceptSystemUpdate);
        Assert.That(result, Is.Not.Null);

        var response = result as OkObjectResult;
        Assert.That(response, Is.Not.Null);
        Assert.That(response.Value, Is.EqualTo(conceptSystemDetail));
        Assert.That(response.StatusCode, Is.EqualTo(200));
    }

    [Test]
    public async Task CanDelete()
    {
        var result = await conceptSystemController.Delete(1);

        await conceptSystemServiceMock.Received(1).Delete(Arg.Is(1));

        Assert.That(result, Is.Not.Null);
        var response = result as OkResult;
        Assert.That(response, Is.Not.Null);
    }
}
