using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bk.Termdat.Business.Resource.Models;
using Bk.Termdat.Business.Services.Interfaces;
using Bk.Termdat.Private.Api.Controllers;
using Bogus;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ClearExtensions;
using NUnit.Framework;

namespace Bk.Termdat.Private.Api.Tests.Controllers;

[TestFixture]
internal class SubjectsControllerTests
{
    private SubjectsController subjectController;

    private readonly ISubjectService subjectServiceMock = Substitute.For<ISubjectService>();
    private IEnumerable<SubjectSummary> subjectSummaries;
    private IEnumerable<SubjectDetail> subjectDetails;
    private SubjectModification subjectModification;

    [SetUp]
    public void SetUp()
    {
        subjectServiceMock.ClearSubstitute();

        var subjectDetailFaker = new Faker<SubjectDetail>();
        subjectDetails = subjectDetailFaker.Generate(3);
        subjectServiceMock.GetAllDetails().Returns(subjectDetails);

        subjectModification = new Faker<SubjectModification>().Generate();
        subjectServiceMock.GetDetail(1).Returns(subjectModification);

        subjectServiceMock.Create(subjectModification).Returns(subjectDetails.First());
        subjectServiceMock.Update(subjectModification).Returns(subjectDetails.First());

        var random = new Randomizer();
        var faker = new Faker<SubjectSummary>().RuleFor(t => t.Name, _ => random.Word());
        subjectSummaries = Enumerable.Range(1, 8).Select(_ => faker.Generate()).ToList();
        subjectServiceMock.GetAll().Returns(subjectSummaries);

        subjectController = new SubjectsController(subjectServiceMock);
    }

    [Test]
    public async Task UpdateShouldReturnSubjectDetail()
    {
        subjectModification.Id = 1;
        var result = await subjectController.Update(1, subjectModification);

        await subjectServiceMock.Received(1).Update(subjectModification);

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        var response = result as OkObjectResult;
        Assert.That(response, Is.Not.Null);
        Assert.That(response.Value, Is.EqualTo(subjectDetails.First()));
        Assert.That(response.StatusCode, Is.EqualTo(200));
    }

    [Test]
    public async Task UpdateShouldReturnBadRequestForWrongId()
    {
        subjectModification.Id = 2;
        var result = await subjectController.Update(1, subjectModification);

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<BadRequestResult>());
        var response = result as BadRequestResult;
        Assert.That(response, Is.Not.Null);
        Assert.That(response.StatusCode, Is.EqualTo(400));
    }

    [Test]
    public async Task UpdateShouldReturnBadRequestForEmptyForm()
    {
        var result = await subjectController.Update(1, null);

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<BadRequestResult>());
        var response = result as BadRequestResult;
        Assert.That(response, Is.Not.Null);
        Assert.That(response.StatusCode, Is.EqualTo(400));
    }

    [Test]
    public async Task CreateShouldReturnSubjectDetail()
    {
        var result = await subjectController.Create(subjectModification);

        await subjectServiceMock.Received(1).Create(subjectModification);

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        var response = result as OkObjectResult;
        Assert.That(response, Is.Not.Null);
        Assert.That(response.Value, Is.EqualTo(subjectDetails.First()));
        Assert.That(response.StatusCode, Is.EqualTo(200));
    }

    [Test]
    public async Task CreateShouldReturnBadRequest()
    {
        var result = await subjectController.Create(null);

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<BadRequestResult>());
        var response = result as BadRequestResult;
        Assert.That(response, Is.Not.Null);
        Assert.That(response.StatusCode, Is.EqualTo(400));
    }

    [Test]
    public async Task ShouldGetDetail()
    {
        var result = await subjectController.GetDetail(1);

        await subjectServiceMock.Received(1).GetDetail(1);
        var ok = result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        var modification = ok.Value as SubjectModification;
        Assert.That(modification, Is.Not.Null);
        Assert.That(modification, Is.EqualTo(subjectModification));
    }

    [Test]
    public async Task ShouldGetAllDetails()
    {
        var result = await subjectController.GetAllDetails();

        await subjectServiceMock.Received(1).GetAllDetails();
        var ok = result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        var details = ok.Value as List<SubjectDetail>;
        Assert.That(details, Is.Not.Null);
        Assert.That(details, Is.EqualTo(subjectDetails));
    }

    [Test]
    public async Task CanDelete()
    {
        var result = await subjectController.Delete(1);

        await subjectServiceMock.Received(1).Delete(Arg.Is(1));
        Assert.That(result is OkResult);
    }

    [Test]
    public async Task ShouldGetAll()
    {
        var result = await subjectController.GetAll();

        await subjectServiceMock.Received().GetAll();

        Assert.That(result, Is.Not.Null);
        var response = result as OkObjectResult;
        Assert.That(response, Is.Not.Null);
        Assert.That(response.Value, Is.Not.Null);
        Assert.That(response.Value, Is.EqualTo(subjectSummaries));
        Assert.That(response.StatusCode, Is.EqualTo(200));
    }
}
