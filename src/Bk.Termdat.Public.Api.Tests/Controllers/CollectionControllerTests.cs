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
internal class CollectionControllerTests
{
    private CollectionController collectionController;
    private ICollectionService collectionServiceMock;
    private readonly Fixture fixture = new();

    [SetUp]
    public void Setup()
    {
        collectionServiceMock = Substitute.For<ICollectionService>();
        collectionController = new CollectionController(collectionServiceMock);
    }

    [Test]
    public async Task GetCollectionsShouldReturnCollections()
    {
        var languageCode = fixture.Create<string>();
        var collectionSummaries = fixture.CreateMany<CollectionSummary>();

        collectionServiceMock.GetAllActiveApi(languageCode).Returns(collectionSummaries);

        var result = await collectionController.GetCollections(languageCode);

        await collectionServiceMock.Received().GetAllActiveApi(languageCode);

        Assert.That(result, Is.Not.Null);
        var response = result as OkObjectResult;
        Assert.That(response, Is.Not.Null);
        Assert.That(response.Value, Is.Not.Null);
        Assert.That(response.Value, Is.EqualTo(collectionSummaries));
        Assert.That(response.StatusCode, Is.EqualTo(200));
    }

    [Test]
    public async Task GetCollectionsShouldReturnBadRequest()
    {
        var result = await collectionController.GetCollections(null);

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<BadRequestResult>());
        var response = result as BadRequestResult;
        Assert.That(response, Is.Not.Null);
        Assert.That(response.StatusCode, Is.EqualTo(400));
    }
}
