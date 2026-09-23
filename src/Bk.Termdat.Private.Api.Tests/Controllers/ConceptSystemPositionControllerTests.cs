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
internal class ConceptSystemPositionControllerTests
{
    private ConceptSystemPositionController conceptSystemPositionController;

    private readonly IConceptSystemPositionService conceptSystemPositionServiceMock = Substitute.For<IConceptSystemPositionService>();
    private IEnumerable<ConceptSystemPositionSummary> conceptSystemPositions;
    private const int CollectionId = 13;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        var random = new Randomizer();
        var faker = new Faker<ConceptSystemPositionSummary>().RuleFor(t => t.Value, _ => random.Word());
        conceptSystemPositions = Enumerable.Range(1, 8).Select(_ => faker.Generate());

        conceptSystemPositionServiceMock.GetByCollectionId(CollectionId).Returns(conceptSystemPositions);
        conceptSystemPositionController = new ConceptSystemPositionController(conceptSystemPositionServiceMock);
    }

    [Test]
    public async Task GetByConceptSystemIdShouldCallDomainService()
    {
        var result = await conceptSystemPositionController.GetByCollectionId(CollectionId);

        await conceptSystemPositionServiceMock.Received().GetByCollectionId(CollectionId);
        Assert.That(result, Is.Not.Null);

        var response = result as OkObjectResult;
        Assert.That(response, Is.Not.Null);
        Assert.That(response.Value, Is.Not.Null);
        Assert.That(response.Value, Is.EqualTo(conceptSystemPositions));
        Assert.That(response.StatusCode, Is.EqualTo(200));
    }
}
