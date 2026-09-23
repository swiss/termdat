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
public class EntryTemplatesControllerTests
{
    private EntryTemplateCreate entryTemplateCreate;
    private EntryTemplatesController entryTemplateController;
    private readonly IEntryTemplateService entryTemplateService = Substitute.For<IEntryTemplateService>();

    [SetUp]
    public void SetUp()
    {
        entryTemplateController = new EntryTemplatesController(entryTemplateService);
        entryTemplateCreate = new Faker<EntryTemplateCreate>().RuleFor(x => x.TemplateName, x => x.Random.String(1, 10)).Generate();
    }

    [TearDown]
    public void TearDown()
    {
        entryTemplateService.ClearSubstitute();
    }

    [Test]
    public async Task Create_WithNull_ShouldReturnBadRequest()
    {
        var result = await entryTemplateController.CreateTemplate(null);

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<BadRequestResult>());
        var response = result as BadRequestResult;
        Assert.That(response, Is.Not.Null);
        Assert.That(response.StatusCode, Is.EqualTo(400));
    }

    [Test]
    public async Task Create_WhenCalled_ShouldReturnOk()
    {
        var result = await entryTemplateController.CreateTemplate(entryTemplateCreate);

        await entryTemplateService.Received(1).Create(entryTemplateCreate);

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<OkResult>());
        var response = result as OkResult;
        Assert.That(response, Is.Not.Null);
        Assert.That(response.StatusCode, Is.EqualTo(200));
    }

    [Test]
    public async Task CreateFromTemplate_WhenCalled_ShouldReturnOk()
    {
        var result = await entryTemplateController.GetTemplateById(1);

        await entryTemplateService.Received(1).GetTemplateById(1);

        Assert.That(result, Is.Not.Null);
        var response = result as OkObjectResult;
        Assert.That(response, Is.Not.Null);
        Assert.That(response.StatusCode, Is.EqualTo(200));
    }

    [Test]
    public async Task Update_WhenCalled_ShouldReturnOk()
    {
        var templateUpdate = new EntryTemplateUpdate { Id = 42 };

        var result = await entryTemplateController.Update(templateUpdate.Id, templateUpdate);

        await entryTemplateService.Received(1).Update(templateUpdate);
        Assert.That(result, Is.Not.Null);
        var response = result as OkResult;
        Assert.That(response, Is.Not.Null);
        Assert.That(response.StatusCode, Is.EqualTo(200));
    }

    [Test]
    public async Task GetForUser_WhenCalled_ShouldReturnOk()
    {
        var result = await entryTemplateController.GetForUser();

        await entryTemplateService.Received(1).GetForUser();

        Assert.That(result, Is.Not.Null);
        var response = result as OkObjectResult;
        Assert.That(response, Is.Not.Null);
        Assert.That(response.StatusCode, Is.EqualTo(200));
    }

    [Test]
    public async Task Delete_WhenCalled_ShouldReturnOk()
    {
        var result = await entryTemplateController.Delete(1);

        await entryTemplateService.Received().Delete(Arg.Is(1));

        Assert.That(result, Is.InstanceOf<OkResult>());
        var response = result as OkResult;
        Assert.That(response, Is.Not.Null);
        Assert.That(response.StatusCode, Is.EqualTo(200));
    }
}
