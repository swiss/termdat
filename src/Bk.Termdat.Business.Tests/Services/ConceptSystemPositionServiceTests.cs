using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Bk.Termdat.Business.Repositories;
using Bk.Termdat.Business.Services;
using Bogus;
using NSubstitute;
using NUnit.Framework;
using DomainModels = Bk.Termdat.Business.Domain.Models;
using ResourceModels = Bk.Termdat.Business.Resource.Models;

namespace Bk.Termdat.Business.Tests.Services;

[TestFixture]
internal class ConceptSystemPositionServiceTests
{
    private ConceptSystemPositionService conceptSystemService;
    private IConceptSystemPositionRepository conceptSystemPositionRepositoryMock;
    private IMapper mapperMock;
    private IEnumerable<DomainModels.ConceptSystemPosition> conceptSystemsPositions;
    private const int CollectionId = 13;

    [SetUp]
    public void SetUp()
    {
        mapperMock = Substitute.For<IMapper>();
        conceptSystemPositionRepositoryMock = Substitute.For<IConceptSystemPositionRepository>();
        var faker = new Faker<DomainModels.ConceptSystemPosition>();
        conceptSystemsPositions = Enumerable.Range(1, 4).Select(_ => faker.Generate());

        conceptSystemService = new ConceptSystemPositionService(mapperMock, conceptSystemPositionRepositoryMock);
    }

    [Test]
    public async Task GetByCollectionIdCallsRepository()
    {
        conceptSystemPositionRepositoryMock.GetByCollectionId(CollectionId).Returns(conceptSystemsPositions);
        var mappedConceptSystems = new List<ResourceModels.ConceptSystemPositionSummary>();
        mapperMock.Map<IEnumerable<ResourceModels.ConceptSystemPositionSummary>>(conceptSystemsPositions).Returns(mappedConceptSystems);

        var result = await conceptSystemService.GetByCollectionId(CollectionId);

        Assert.That(result, Is.EqualTo(mappedConceptSystems));
    }

}
