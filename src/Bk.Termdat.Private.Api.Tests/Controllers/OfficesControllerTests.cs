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
internal class OfficesControllerTests
{
    private OfficesController officeController;

    private readonly IOfficeService officeServiceMock = Substitute.For<IOfficeService>();
    private IEnumerable<OfficeSummary> offices;
    private IEnumerable<OfficeDetail> officeDetails;
    private OfficeCreate officeCreate;
    private OfficeUpdate officeUpdate;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        var random = new Randomizer();
        var faker = new Faker<OfficeSummary>().RuleFor(t => t.Name, _ => random.Word());
        offices = Enumerable.Range(1, 8).Select(_ => faker.Generate());

        var fakerDetails = new Faker<OfficeDetail>().RuleFor(t => t.Name, _ => random.Word());
        officeDetails = Enumerable.Range(1, 8).Select(_ => fakerDetails.Generate()).ToList();

        officeCreate = new Faker<OfficeCreate>().RuleFor(o => o.Code, _ => random.String(1, 5));
        officeUpdate = new Faker<OfficeUpdate>().RuleFor(o => o.Code, _ => random.String(1, 5)).RuleFor(o => o.Id, _ => random.Number(1, 5000));

        officeServiceMock.GetAll().ReturnsForAnyArgs(offices);
        officeServiceMock.GetAllDetails().ReturnsForAnyArgs(officeDetails);
        officeServiceMock.GetDetails(1).Returns(officeUpdate);
        officeController = new OfficesController(officeServiceMock);
    }

    [Test]
    public async Task CreateShouldReturnBadRequest()
    {
        var result = await officeController.Create(null);

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<BadRequestResult>());
    }

    [Test]
    public async Task CreateShouldReturnOkResult()
    {
        var officeDetail = officeDetails.First();
        officeServiceMock.Create(officeCreate).Returns(officeDetail);

        var result = await officeController.Create(officeCreate);

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        var response = result as OkObjectResult;
        Assert.That(response, Is.Not.Null);
        Assert.That(response.Value, Is.Not.Null);
        Assert.That(response.Value, Is.InstanceOf<OfficeDetail>());
        var detail = response.Value as OfficeDetail;
        Assert.That(detail, Is.EqualTo(officeDetail));
    }

    [Test]
    public async Task UpateShouldReturnBadRequest()
    {
        var result = await officeController.Update(1, null);

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<BadRequestResult>());
    }

    [Test]
    public async Task UpdateShouldReturnOkResult()
    {
        var officeDetail = officeDetails.First();
        officeServiceMock.Update(officeUpdate).Returns(officeDetail);

        var result = await officeController.Update(officeUpdate.Id, officeUpdate);

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        var response = result as OkObjectResult;
        Assert.That(response, Is.Not.Null);
        Assert.That(response.Value, Is.Not.Null);
        Assert.That(response.Value, Is.InstanceOf<OfficeDetail>());
        var detail = response.Value as OfficeDetail;
        Assert.That(detail, Is.EqualTo(officeDetail));
    }

    [Test]
    public async Task GetAllShouldCallOfficeService()
    {
        var result = await officeController.GetAll();

        await officeServiceMock.Received().GetAll();

        Assert.That(result, Is.Not.Null);
        var response = result as OkObjectResult;
        Assert.That(response, Is.Not.Null);
        Assert.That(response.Value, Is.Not.Null);
        Assert.That(response.Value, Is.EqualTo(offices));
        Assert.That(response.StatusCode, Is.EqualTo(200));
    }

    [Test]
    public async Task GetDetailsShouldCallOfficeService()
    {
        var result = await officeController.GetDetails(1);

        await officeServiceMock.Received().GetDetails(1);

        Assert.That(result, Is.Not.Null);
        var response = result as OkObjectResult;
        Assert.That(response, Is.Not.Null);
        Assert.That(response.Value, Is.Not.Null);
        Assert.That(response.Value, Is.EqualTo(officeUpdate));
        Assert.That(response.StatusCode, Is.EqualTo(200));
    }

    [Test]
    public async Task GetAllDetailsShouldCallOfficeService()
    {
        var result = await officeController.GetAllDetails();

        await officeServiceMock.Received().GetAllDetails();

        Assert.That(result, Is.Not.Null);
        var response = result as OkObjectResult;
        Assert.That(response, Is.Not.Null);
        Assert.That(response.Value, Is.Not.Null);
        Assert.That(response.Value, Is.EqualTo(officeDetails));
        Assert.That(response.StatusCode, Is.EqualTo(200));
    }

    [Test]
    public async Task CanDelete()
    {
        var result = await officeController.Delete(1);

        await officeServiceMock.Received().Delete(Arg.Is(1));

        Assert.That(result, Is.InstanceOf<OkResult>());
    }
}
