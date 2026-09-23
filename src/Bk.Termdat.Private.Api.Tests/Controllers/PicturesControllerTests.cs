using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Bk.Termdat.Business.Domain.Models;
using Bk.Termdat.Business.Services.Interfaces;
using Bk.Termdat.Private.Api.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using NSubstitute;
using NUnit.Framework;

namespace Bk.Termdat.Private.Api.Tests.Controllers;

[TestFixture]
public class PicturesControllerTests
{
    private IPictureService service;
    private PicturesController controller;
    private Picture picture;

    [SetUp]
    public void SetUp()
    {
        picture = new Picture
        {
            EntityTag = "6C57BA4E5B0E50BAB603F4E563AC039F",
            ContentType = "image/jpeg",
            File = new PictureFile { Value = Encoding.ASCII.GetBytes("TEST IMAGE") }
        };
        service = Substitute.For<IPictureService>();
        service.GetPicture(Arg.Is(1)).Returns(picture);

        controller = new PicturesController(service)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
    }

    [Test]
    public async Task GetPictureReturnsCorrectPicture()
    {
        var result = await controller.GetPicture(1);

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<FileContentResult>());
        var fileContentResult = result as FileContentResult;
        Assert.That(fileContentResult, Is.Not.Null);
        Assert.That(fileContentResult.ContentType, Is.EqualTo(picture.ContentType));
        Assert.That(fileContentResult.FileContents, Is.EqualTo(picture.File.Value));
        Assert.That(fileContentResult.EntityTag, Is.Not.Null);
        Assert.That(fileContentResult.EntityTag.Tag.Value, Is.EqualTo($"\"{picture.EntityTag}\""));

        await service.Received(1).GetPicture(Arg.Is(1));
    }

    [Test]
    public async Task GetPictureReturnsNotFoundForNonExistingPictures()
    {
        var result = await controller.GetPicture(2);

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<NotFoundResult>());
        Assert.That(((NotFoundResult)result).StatusCode, Is.EqualTo(404));
    }

    [Test]
    public async Task GetPictureReturns304WhenEtagMatchesOnRequestHeaders()
    {
        controller.HttpContext.Request.Headers.Add(new KeyValuePair<string, StringValues>("If-None-Match", picture.EntityTag));
        var result = await controller.GetPicture(1);

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<StatusCodeResult>());
        Assert.That(((StatusCodeResult)result).StatusCode, Is.EqualTo(304));

        await service.Received(1).GetPicture(Arg.Is(1));
    }
}
