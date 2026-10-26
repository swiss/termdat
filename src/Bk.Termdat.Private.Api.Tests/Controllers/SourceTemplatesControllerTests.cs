using System.Collections.Generic;
using System.Threading.Tasks;
using Bk.Termdat.Business.Resource.Models;
using Bk.Termdat.Business.Services.Interfaces;
using Bk.Termdat.Private.Api.Controllers;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NUnit.Framework;

namespace Bk.Termdat.Private.Api.Tests.Controllers;

[TestFixture]
internal class SourceTemplatesControllerTests
{
    private SourceTemplatesController controller;
    private ISourceTemplateService sourceTemplateServiceMock;
    private SourceTemplateDefaults sourceTemplateDefaults;
    private IList<SourceTemplateSummary> sourceTemplates;
    private IList<string> sourceTemplateTexts;

    [SetUp]
    public void SetUp()
    {
        sourceTemplateDefaults = new SourceTemplateDefaults();

        sourceTemplates = new List<SourceTemplateSummary>()
        {
            new()
            {
                Id = 1,
                TextDe = "Text De",
                TextFr = "Text Fr",
                TextIt = "Text It",
                TextEn = "Text En",
                TextRm = "Text Rm",
            }
        };

        sourceTemplateTexts = new List<string>()
        {
            "Text De1",
            "Text De2",
            "Text De3",
            "Text De4",
            "Text De5",
        };

        sourceTemplateServiceMock = Substitute.For<ISourceTemplateService>();
        sourceTemplateServiceMock.GetAllDefaults().Returns(sourceTemplateDefaults);
        sourceTemplateServiceMock.GetAllForCurrentUser().Returns(sourceTemplates);
        sourceTemplateServiceMock.GetAllForCurrentUserAsText("de").Returns(sourceTemplateTexts);
        controller = new SourceTemplatesController(sourceTemplateServiceMock);
    }

    [Test]
    public void CanGetAllDefaults()
    {
        var result = controller.GetAllDefaults();
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        Assert.That(((OkObjectResult)result).Value, Is.EqualTo(sourceTemplateDefaults));
    }

    [Test]
    public async Task GetAllForCurrentUser_WhenCalled_ReturnsData()
    {
        var result = await controller.GetAllForCurrentUser();
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        Assert.That(((OkObjectResult)result).Value, Is.EqualTo(sourceTemplates));
    }

    [Test]
    public async Task GetAllForCurrentUserAsText_WhenCalled_ReturnsData()
    {
        var result = await controller.GetAllForCurrentUserAsText("de");
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        Assert.That(((OkObjectResult)result).Value, Is.EqualTo(sourceTemplateTexts));
    }

    [Test]
    public async Task Create_WhenCalled_ReturnsData()
    {
        var newSourceTemplate = new SourceTemplateModification()
        {
            TextDe = "Text De",
            TextFr = "Text Fr",
            TextIt = "Text It",
            TextEn = "Text En",
            TextRm = "Text Rm"
        };

        var result = await controller.Create(newSourceTemplate);
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<OkResult>());
    }

    [Test]
    public async Task Update_WhenCalled_ShouldCallService()
    {
        var updatedSourceTemplate = new SourceTemplateModification()
        {
            TextDe = "Text De",
            TextFr = "Text Fr",
            TextIt = "Text It",
            TextEn = "Text En",
            TextRm = "Text Rm"
        };

        var result = await controller.Update(1, updatedSourceTemplate);
        await sourceTemplateServiceMock.Received(1).Update(1, updatedSourceTemplate);

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<OkResult>());
    }

    [Test]
    public async Task Delete_WhenCalled_ShouldCallService()
    {
        var result = await controller.Delete(1);

        await sourceTemplateServiceMock.Received(1).Delete(1);

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<OkResult>());
    }
}
