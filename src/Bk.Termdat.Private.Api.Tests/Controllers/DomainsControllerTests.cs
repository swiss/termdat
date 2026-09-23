using System.Threading.Tasks;
using Bk.Termdat.Business.Resource.Models;
using Bk.Termdat.Business.Services.Interfaces;
using Bk.Termdat.Private.Api.Controllers;
using Bogus;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NUnit.Framework;

namespace Bk.Termdat.Private.Api.Tests.Controllers;

public class DomainsControllerTests
{
    private DomainsController domainController;

    private readonly IDomainService domainServiceMock = Substitute.For<IDomainService>();
    private DomainModification domainModification;
    private DomainDetail domainDetail;

    [SetUp]
    public void SetUp()
    {
        domainController = new DomainsController(domainServiceMock);
        domainModification = new DomainModification
        {
            Id = 1,
            SubjectId = 2,
            DescriptionDe = "de",
            DescriptionEn = "en",
            DescriptionFr = "fr",
            DescriptionIt = "it",
            DescriptionRm = "rm",
        };
        domainDetail = new Faker<DomainDetail>().Generate();
    }

    [Test]
    public async Task GetAllDescriptionsShouldCallDomainService()
    {
        var result = await domainController.GetAllDescriptions();

        await domainServiceMock.Received(1).GetAllDescriptions();
        Assert.That(result is OkObjectResult);
    }

    [Test]
    public async Task UpdateShouldReturnBadRequestForWrongId()
    {
        var result = await domainController.Update(2, domainModification);

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<BadRequestResult>());
        var response = result as BadRequestResult;
        Assert.That(response, Is.Not.Null);
        Assert.That(response.StatusCode, Is.EqualTo(400));
    }

    [Test]
    public async Task UpdateShouldReturnBadRequestForEmptyModel()
    {
        var result = await domainController.Update(1, null);

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<BadRequestResult>());
        var response = result as BadRequestResult;
        Assert.That(response, Is.Not.Null);
        Assert.That(response.StatusCode, Is.EqualTo(400));
    }

    [Test]
    public async Task UpdateShouldCallDomainService()
    {
        domainServiceMock.Update(domainModification).Returns(domainDetail);
        var result = await domainController.Update(1, domainModification);

        await domainServiceMock.Received().Update(domainModification);

        Assert.That(result, Is.Not.Null);
        var response = result as OkObjectResult;
        Assert.That(response, Is.Not.Null);
        Assert.That(response.Value, Is.EqualTo(domainDetail));
        Assert.That(response.StatusCode, Is.EqualTo(200));
    }

    [Test]
    public async Task CanDelete()
    {
        var result = await domainController.Delete(1);

        await domainServiceMock.Received(1).Delete(Arg.Is(1));
        Assert.That(result is OkResult);
    }
}
