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
internal class CollectionsControllerTests
{
    private CollectionsController collectionController;

    private readonly IAuthorizationService authorizationServiceMock = Substitute.For<IAuthorizationService>();
    private ICollectionService collectionServiceMock;
    private IEnumerable<CollectionSummary> collections;
    private IEnumerable<CollectionDetail> collectionDetails;
    private CollectionDetail collectionDetail;
    private CollectionCreate collectionCreate;
    private CollectionUpdate collectionUpdate;

    [SetUp]
    public void SetUp()
    {
        var random = new Randomizer();
        var faker = new Faker<CollectionSummary>().RuleFor(t => t.Name, _ => random.Word());
        collections = Enumerable.Range(1, 8).Select(_ => faker.Generate()).ToList();
        var fakerDetail = new Faker<CollectionDetail>().RuleFor(t => t.Name, _ => random.Word());
        collectionDetails = Enumerable.Range(1, 8).Select(_ => fakerDetail.Generate()).ToList();
        collectionDetail = fakerDetail.Generate();
        collectionCreate = new Faker<CollectionCreate>().RuleFor(x => x.Code, f => f.Random.String(1, 10)).Generate();
        collectionUpdate = new Faker<CollectionUpdate>().RuleFor(x => x.Code, f => f.Random.String(1, 10)).Generate();

        collectionServiceMock = Substitute.For<ICollectionService>();
        collectionServiceMock.GetAll().ReturnsForAnyArgs(collections);
        collectionServiceMock.GetAllForCurrentUser().ReturnsForAnyArgs(collections);
        collectionServiceMock.GetAllActive().ReturnsForAnyArgs(collections);
        collectionServiceMock.GetAllDetails().ReturnsForAnyArgs(collectionDetails);
        collectionServiceMock.GetAllForModification().ReturnsForAnyArgs(collections);
        collectionServiceMock.GetDetail(0).ReturnsForAnyArgs(collectionUpdate);
        collectionServiceMock.GetById(Arg.Any<int>()).ReturnsForAnyArgs(collectionDetails.First());

        authorizationServiceMock.IsAdmin().Returns(false);

        collectionController = new CollectionsController(collectionServiceMock, authorizationServiceMock);
    }

    [Test]
    public async Task GetAllActiveShouldCallClassificationService()
    {
        var result = await collectionController.GetAllActive();

        await collectionServiceMock.Received().GetAllActive();

        Assert.That(result, Is.Not.Null);
        var response = result as OkObjectResult;
        Assert.That(response, Is.Not.Null);
        Assert.That(response.Value, Is.Not.Null);
        Assert.That(response.Value, Is.EqualTo(collections));
        Assert.That(response.StatusCode, Is.EqualTo(200));
    }

    [Test]
    public async Task GetAll_WithoutAdminOrWriterOrSuperReaderRole_ShouldCallCollectionServiceAndReturnActiveCollections()
    {
        authorizationServiceMock.IsAdmin().Returns(false);
        authorizationServiceMock.IsSuperReader().Returns(false);
        authorizationServiceMock.IsWriter().Returns(false);
        var result = await collectionController.GetAll();

        await collectionServiceMock.Received().GetAllActive();

        Assert.That(result, Is.Not.Null);
        var response = result as OkObjectResult;
        Assert.That(response, Is.Not.Null);
        Assert.That(response.Value, Is.Not.Null);
        Assert.That(response.Value, Is.EqualTo(collections));
        Assert.That(response.StatusCode, Is.EqualTo(200));
    }

    [Test]
    public async Task GetAllShouldCallCollectionServiceGetAllAsAdmin()
    {
        authorizationServiceMock.IsAdmin().Returns(true);
        var result = await collectionController.GetAll();

        await collectionServiceMock.Received().GetAll();

        Assert.That(result, Is.Not.Null);
        var response = result as OkObjectResult;
        Assert.That(response, Is.Not.Null);
        Assert.That(response.Value, Is.Not.Null);
        Assert.That(response.Value, Is.EqualTo(collections));
        Assert.That(response.StatusCode, Is.EqualTo(200));
    }

    [Test]
    public async Task GetAll_AsSuperReader_ShouldCallCollectionService()
    {
        authorizationServiceMock.IsSuperReader().Returns(true);
        var result = await collectionController.GetAll();

        await collectionServiceMock.Received().GetAllForCurrentUser();

        Assert.That(result, Is.Not.Null);
        var response = result as OkObjectResult;
        Assert.That(response, Is.Not.Null);
        Assert.That(response.Value, Is.Not.Null);
        Assert.That(response.Value, Is.EqualTo(collections));
        Assert.That(response.StatusCode, Is.EqualTo(200));
    }

    [Test]
    public async Task GetAll_AsWriter_ShouldCallCollectionService()
    {
        authorizationServiceMock.IsWriter().Returns(true);
        var result = await collectionController.GetAll();

        await collectionServiceMock.Received().GetAllForCurrentUser();

        Assert.That(result, Is.Not.Null);
        var response = result as OkObjectResult;
        Assert.That(response, Is.Not.Null);
        Assert.That(response.Value, Is.Not.Null);
        Assert.That(response.Value, Is.EqualTo(collections));
        Assert.That(response.StatusCode, Is.EqualTo(200));
    }

    [Test]
    public async Task GetAllDetailsShouldCallCollectionService()
    {
        var result = await collectionController.GetAllDetails();

        await collectionServiceMock.Received().GetAllDetails();

        Assert.That(result, Is.Not.Null);
        var response = result as OkObjectResult;
        Assert.That(response, Is.Not.Null);
        Assert.That(response.Value, Is.Not.Null);
        Assert.That(response.Value, Is.EqualTo(collectionDetails));
        Assert.That(response.StatusCode, Is.EqualTo(200));
    }

    [Test]
    public async Task GetDetailShouldReturnCollectionUpdate()
    {
        var result = await collectionController.GetDetail(1);

        await collectionServiceMock.Received().GetDetail(1);

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        var response = result as OkObjectResult;
        Assert.That(response, Is.Not.Null);
        Assert.That(response.Value, Is.Not.Null);
        Assert.That(response.Value, Is.EqualTo(collectionUpdate));
        Assert.That(response.StatusCode, Is.EqualTo(200));
    }

    [Test]
    public async Task CreateShouldReturnBadRequest()
    {
        var result = await collectionController.Create(null);

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<BadRequestResult>());
        var response = result as BadRequestResult;
        Assert.That(response, Is.Not.Null);
        Assert.That(response.StatusCode, Is.EqualTo(400));
    }

    [Test]
    public async Task CreateShouldCallCollectionService()
    {
        collectionServiceMock.Create(collectionCreate).Returns(collectionDetail);

        var result = await collectionController.Create(collectionCreate);
        await collectionServiceMock.Received().Create(collectionCreate);
        Assert.That(result, Is.Not.Null);

        var response = result as OkObjectResult;
        Assert.That(response, Is.Not.Null);
        Assert.That(response.Value, Is.EqualTo(collectionDetail));
        Assert.That(response.StatusCode, Is.EqualTo(200));
    }

    [Test]
    public async Task UpdateShouldReturnBadRequestForEmptyForm()
    {
        var result = await collectionController.Update(1, null);

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<BadRequestResult>());
        var response = result as BadRequestResult;
        Assert.That(response, Is.Not.Null);
        Assert.That(response.StatusCode, Is.EqualTo(400));
    }

    [Test]
    public async Task UpdateShouldReturnBadRequestForWrongId()
    {
        collectionUpdate.Id = 2;
        var result = await collectionController.Update(1, collectionUpdate);

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<BadRequestResult>());
        var response = result as BadRequestResult;
        Assert.That(response, Is.Not.Null);
        Assert.That(response.StatusCode, Is.EqualTo(400));
    }

    [Test]
    public async Task UpdateShouldCallCollectionService()
    {
        collectionServiceMock.Update(collectionUpdate).Returns(collectionDetail);
        collectionUpdate.Id = 1;
        var result = await collectionController.Update(1, collectionUpdate);

        await collectionServiceMock.Received().Update(collectionUpdate);
        Assert.That(result, Is.Not.Null);

        var response = result as OkObjectResult;
        Assert.That(response, Is.Not.Null);
        Assert.That(response.Value, Is.EqualTo(collectionDetail));
        Assert.That(response.StatusCode, Is.EqualTo(200));
    }

    [Test]
    public async Task CanSoftDelete()
    {
        var result = await collectionController.Delete(1, soft: true);

        await collectionServiceMock.Received(1).Delete(Arg.Is(1), Arg.Is(true));

        Assert.That(result, Is.InstanceOf<OkResult>());
    }

    [Test]
    public async Task CanDelete()
    {
        var result = await collectionController.Delete(1, soft: false);

        await collectionServiceMock.Received(1).Delete(Arg.Is(1), Arg.Is(false));

        Assert.That(result, Is.InstanceOf<OkResult>());
    }

    [Test]
    public async Task CanRestore()
    {
        var result = await collectionController.Restore(1);

        await collectionServiceMock.Received(1).Restore(Arg.Is(1));
        await collectionServiceMock.Received(1).GetById(Arg.Is(1));

        Assert.That(result, Is.Not.Null);
        var response = result as OkObjectResult;
        Assert.That(response, Is.Not.Null);
        Assert.That(response.Value, Is.Not.Null);
        Assert.That(response.Value, Is.EqualTo(collectionDetails.First()));
    }

    [Test]
    public async Task GetCollectionsForModification_WhenCalled_ReturnsModificationCollections()
    {
        var result = await collectionController.GetCollectionsForModification();

        await collectionServiceMock.Received(1).GetAllForModification();

        Assert.That(result, Is.Not.Null);
        var response = result as OkObjectResult;
        Assert.That(response, Is.Not.Null);
        Assert.That(response.Value, Is.Not.Null);
        Assert.That(response.Value, Is.EqualTo(collections));
        Assert.That(response.StatusCode, Is.EqualTo(200));
    }
}
