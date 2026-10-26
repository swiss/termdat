using System.Threading.Tasks;
using AutoFixture;
using Bk.Termdat.Business.Public.Api.Resource.Models;
using Bk.Termdat.Business.Public.Api.Services;
using Bk.Termdat.Public.Api.Controllers;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ClearExtensions;
using NUnit.Framework;

namespace Bk.Termdat.Public.Api.Tests.Controllers;

[TestFixture]
internal class EntryControllerTests
{
    private readonly IEntryService entryService = Substitute.For<IEntryService>();
    private readonly IAnalyticsService analyticsService = Substitute.For<IAnalyticsService>();
    private readonly Fixture fixture = new();

    private EntryController entryController;

    [SetUp]
    public void Setup()
    {
        entryController = new EntryController(entryService, analyticsService);
    }

    [TearDown]
    public void TearDown()
    {
        entryService.ClearSubstitute();
        analyticsService.ClearSubstitute();
    }

    [Test]
    public async Task GetEntriesShouldReturnEntryDetails()
    {
        var getEntriesParameter = fixture.Create<GetEntriesParameter>();
        var entryDetails = fixture.CreateMany<EntryDetail>();

        entryService.GetByIds(getEntriesParameter.EntryIds, getEntriesParameter.InLanguageCode, getEntriesParameter.OutLanguageCode).Returns(entryDetails);

        var result = await entryController.GetEntries(getEntriesParameter);

        await entryService.Received(1).GetByIds(getEntriesParameter.EntryIds, getEntriesParameter.InLanguageCode, getEntriesParameter.OutLanguageCode);

        Assert.That(result, Is.Not.Null);
        var response = result as OkObjectResult;
        Assert.That(response, Is.Not.Null);
        Assert.That(response.Value, Is.Not.Null);
        Assert.That(response.Value, Is.EqualTo(entryDetails));
        Assert.That(response.StatusCode, Is.EqualTo(200));
    }

    [Test]
    public async Task Search_ShouldTrackEvent()
    {
        await entryController.GetEntries(new GetEntriesParameter());

        await analyticsService.Received(1).TrackEvent("Public Search", "Perform", Arg.Any<string?>());
    }
}
