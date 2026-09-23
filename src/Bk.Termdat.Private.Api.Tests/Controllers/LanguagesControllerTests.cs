using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
internal class LanguagesControllerTests
{
    private LanguagesController languageController;
    private IEnumerable<LanguageSummary> translations;
    private IEnumerable<LanguageDetail> languageDetails;
    private LanguageDetail languageDetail;
    private LanguageCreate languageCreate;
    private LanguageUpdate languageUpdate;

    private ILanguageService languageServiceMock;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        languageServiceMock = Substitute.For<ILanguageService>();

        var random = new Randomizer();
        var faker = new Faker<LanguageSummary>().RuleFor(t => t.Name, _ => random.Word());
        translations = Enumerable.Range(1, 8).Select(_ => faker.Generate());

        var fakerDetail = new Faker<LanguageDetail>().RuleFor(t => t.Name, _ => random.Word());
        languageDetails = Enumerable.Range(1, 8).Select(_ => fakerDetail.Generate());

        languageDetail = fakerDetail.Generate();
        languageCreate = new Faker<LanguageCreate>().RuleFor(x => x.IsoCode, f => f.Random.String(1, 10)).Generate();
        languageUpdate = new Faker<LanguageUpdate>().RuleFor(x => x.IsoCode, f => f.Random.String(1, 10)).Generate();

        languageServiceMock.GetTranslationByIsoCode(null).ReturnsForAnyArgs(translations);
        languageServiceMock.GetAllDetails().ReturnsForAnyArgs(languageDetails);

        languageServiceMock.GetDetail(0).ReturnsForAnyArgs(languageUpdate);

        languageController = new LanguagesController(languageServiceMock);
    }

    [Test]
    public async Task GetLanguageTranslationsForIsoCode()
    {
        var result = await languageController.GetByIsoCode("de") as OkObjectResult;
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Value, Is.EqualTo(translations));
    }

    [Test]
    public async Task GetLanguageTranslationsWithoutAnIsoCodeReturnsBadRequest()
    {
        var result = await languageController.GetByIsoCode(null);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.GetType().IsAssignableFrom(typeof(BadRequestObjectResult)));

        var badRequest = result as BadRequestObjectResult;

        Assert.That(badRequest, Is.Not.Null);
        Assert.That(badRequest.StatusCode, Is.EqualTo((int)HttpStatusCode.BadRequest));
        Assert.That(badRequest.Value, Is.Not.Null);
        Assert.That(badRequest.Value.GetType().IsAssignableFrom(typeof(ArgumentNullException)));
    }

    [Test]
    public async Task GetAllDetailsShouldCallLanguageService()
    {
        var result = await languageController.GetAllDetails();

        await languageServiceMock.Received().GetAllDetails();

        Assert.That(result, Is.Not.Null);
        var response = result as OkObjectResult;
        Assert.That(response, Is.Not.Null);
        Assert.That(response.Value, Is.Not.Null);
        Assert.That(response.Value, Is.EqualTo(languageDetails));
        Assert.That(response.StatusCode, Is.EqualTo(200));
    }

    [Test]
    public async Task CreateShouldReturnBadRequest()
    {
        var result = await languageController.Create(null);

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<BadRequestResult>());
        var response = result as BadRequestResult;
        Assert.That(response, Is.Not.Null);
        Assert.That(response.StatusCode, Is.EqualTo(400));
    }

    [Test]
    public async Task CreateShouldCallCollectionService()
    {
        languageServiceMock.Create(languageCreate).Returns(languageDetail);

        var result = await languageController.Create(languageCreate);
        await languageServiceMock.Received().Create(languageCreate);
        Assert.That(result, Is.Not.Null);

        var response = result as OkObjectResult;
        Assert.That(response, Is.Not.Null);
        Assert.That(response.Value, Is.EqualTo(languageDetail));
        Assert.That(response.StatusCode, Is.EqualTo(200));
    }

    [Test]
    public async Task UpdateShouldReturnBadRequestForEmptyForm()
    {
        var result = await languageController.Update(1, null);

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<BadRequestResult>());
        var response = result as BadRequestResult;
        Assert.That(response, Is.Not.Null);
        Assert.That(response.StatusCode, Is.EqualTo(400));
    }

    [Test]
    public async Task UpdateShouldReturnBadRequestForWrongId()
    {
        languageUpdate.Id = 2;
        var result = await languageController.Update(1, languageUpdate);

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<BadRequestResult>());
        var response = result as BadRequestResult;
        Assert.That(response, Is.Not.Null);
        Assert.That(response.StatusCode, Is.EqualTo(400));
    }

    [Test]
    public async Task UpdateShouldCallCollectionService()
    {
        languageServiceMock.Update(languageUpdate).Returns(languageDetail);
        languageUpdate.Id = 1;
        var result = await languageController.Update(1, languageUpdate);

        await languageServiceMock.Received().Update(languageUpdate);
        Assert.That(result, Is.Not.Null);

        var response = result as OkObjectResult;
        Assert.That(response, Is.Not.Null);
        Assert.That(response.Value, Is.EqualTo(languageDetail));
        Assert.That(response.StatusCode, Is.EqualTo(200));
    }
}
