using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using AutoMapper;
using Bk.Termdat.Business.Domain.Models;
using Bk.Termdat.Business.Public.Api.Resource.Models;
using Bk.Termdat.Business.Public.Api.Services;
using Bk.Termdat.Business.Repositories;
using Bk.Termdat.CrossCutting.Exceptions;
using NSubstitute;
using NUnit.Framework;

namespace Bk.Termdat.Business.Public.Api.Tests.Services;

[TestFixture]
public class CollectionServiceTest
{
    private ICollectionService collectionService;
    private ICollectionRepository collectionRepositoryMock;
    private ILanguageValidationService languageValidationServiceMock;
    private IMapper mapperMock;
    private Fixture fixture;

    [SetUp]
    public void Setup()
    {
        fixture = new();
        fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => fixture.Behaviors.Remove(b));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        collectionRepositoryMock = Substitute.For<ICollectionRepository>();
        languageValidationServiceMock = Substitute.For<ILanguageValidationService>();
        mapperMock = Substitute.For<IMapper>();
        collectionService = new CollectionService(collectionRepositoryMock, languageValidationServiceMock, mapperMock);
    }

    [Test]
    public async Task GetAllActiveShouldReturnActiveCollections()
    {
        var collections = Enumerable.Range(1, 3).Select(id => new Collection { Id = id });
        collectionRepositoryMock.GetAllActiveApi().Returns(collections);

        var isoCode = fixture.Create<string>();
        var languageId = fixture.Create<int>();
        languageValidationServiceMock.ValidateLanguageIsoCode(isoCode).Returns(languageId);

        var collectionSummaries = fixture.CreateMany<CollectionSummary>();
        mapperMock.Map(collections, Arg.Any<Action<IMappingOperationOptions<object, IEnumerable<CollectionSummary>>>>())
            .Returns(collectionSummaries);

        var result = await collectionService.GetAllActiveApi(isoCode);

        await collectionRepositoryMock.Received(1).GetAllActiveApi();
        Assert.That(result, Is.EqualTo(collectionSummaries));
    }

    [Test]
    public async Task GetAllActiveShouldThrow()
    {
        var isoCode = fixture.Create<string>();
        languageValidationServiceMock.When(x => x.ValidateLanguageIsoCode(isoCode)).Do(_ => throw new BusinessValidationException(string.Empty, string.Empty));

        Assert.ThrowsAsync<BusinessValidationException>(() => collectionService.GetAllActiveApi(isoCode));
        await collectionRepositoryMock.DidNotReceive().GetAllActiveApi();
    }
}
