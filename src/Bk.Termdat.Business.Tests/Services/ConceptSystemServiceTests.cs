using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Bk.Termdat.Business.Domain;
using Bk.Termdat.Business.Domain.Models;
using Bk.Termdat.Business.Domain.Projections;
using Bk.Termdat.Business.Repositories;
using Bk.Termdat.Business.Resource.Models;
using Bk.Termdat.Business.Services;
using Bk.Termdat.Business.Services.Interfaces;
using Bk.Termdat.CrossCutting.Exceptions;
using Bogus;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using NUnit.Framework;

namespace Bk.Termdat.Business.Tests.Services;

[TestFixture]
internal class ConceptSystemServiceTests
{
    private ConceptSystemService service;
    private IConceptSystemRepository conceptSystemRepositoryMock;
    private ICollectionRepository collectionRepositoryMock;
    private IEntryRepository entryRepositoryMock;
    private IUnitOfWork unitOfWorkMock;
    private IIdentityService identityServiceMock;
    private IMapper mapperMock;
    private IEnumerable<ConceptSystem> conceptSystems;
    private IEnumerable<ConceptSystemWithEntryCount> conceptSystemsWithEntryCount;
    private IEnumerable<ConceptSystemSummary> conceptSystemSummaries;
    private IEnumerable<ConceptSystemDetail> conceptSystemDetails;
    private ConceptSystem conceptSystem;
    private ConceptSystemDetail conceptSystemDetail;
    private ConceptSystemCreate conceptSystemCreate;
    private ConceptSystemUpdate conceptSystemUpdate;
    private const string CurrentEmail = "Its@me.ch";


    [SetUp]
    public void Setup()
    {
        collectionRepositoryMock = Substitute.For<ICollectionRepository>();
        conceptSystemRepositoryMock = Substitute.For<IConceptSystemRepository>();
        entryRepositoryMock = Substitute.For<IEntryRepository>();
        identityServiceMock = Substitute.For<IIdentityService>();
        unitOfWorkMock = Substitute.For<IUnitOfWork>();
        mapperMock = Substitute.For<IMapper>();

        conceptSystems = Enumerable.Range(1, 8).Select(_ => new Faker<ConceptSystem>().Generate()).ToArray();
        conceptSystemsWithEntryCount = conceptSystems.Select(cs => new ConceptSystemWithEntryCount
        {
            ConceptSystem = cs,
            EntryCount = 1
        });
        conceptSystemSummaries = Enumerable.Range(1, 8).Select(_ => new Faker<ConceptSystemSummary>().Generate());
        conceptSystemDetails = Enumerable.Range(1, 8).Select(_ => new Faker<ConceptSystemDetail>().Generate());

        conceptSystemCreate = new()
        {
            CollectionId = 10,
            Title = "My new CS",
            Positions = new List<ConceptSystemPositionCreate>
            {
                new()
                {
                    Value = "pos 1"
                },
                new()
                {
                    Value = "pos 2"
                }
            }
        };

        conceptSystemUpdate = new()
        {
            Id = 1,
            CollectionId = 10,
            Title = "My updated CS",
            Positions = new List<ConceptSystemPositionUpdate>
            {
                new()
                {
                    Id = 100,
                    Value = "pos 1"
                },
                new()
                {
                    Value = "pos 2"
                }
            }
        };

        conceptSystem = new()
        {
            Id = 1,
            Title = "My CS",
            CollectionId = 10,
            Collection = new()
            {
                Id = 10,
                Code = "Col1"
            },
            ConceptSystemPositions = new List<ConceptSystemPosition>
            {
                new()
                {
                    SortIndex = 1,
                    Id = 100,
                    Value = "pos 2"
                },
                new()
                {
                    SortIndex = 0,
                    Id = 101,
                    Value = "pos 1"
                }
            }
        };

        conceptSystemDetail = new()
        {
            Id = 1,
            Title = "My updated CS",
            Collection = "Col1"
        };

        conceptSystemRepositoryMock.GetAll().ReturnsForAnyArgs(conceptSystems);
        conceptSystemRepositoryMock.GetAllDetails().ReturnsForAnyArgs(conceptSystemsWithEntryCount);
        conceptSystemRepositoryMock.GetByIdForEdit(1).Returns(conceptSystem);

        collectionRepositoryMock.GetById(conceptSystem.CollectionId).Returns(conceptSystem.Collection);

        mapperMock.Map<IEnumerable<ConceptSystemSummary>>(conceptSystems).Returns(conceptSystemSummaries);
        mapperMock.Map<IEnumerable<ConceptSystemDetail>>(conceptSystemsWithEntryCount).Returns(conceptSystemDetails);
        mapperMock.Map<ConceptSystemUpdate>(conceptSystem).Returns(conceptSystemUpdate);
        mapperMock.Map<ConceptSystemDetail>(conceptSystem).Returns(conceptSystemDetail);
        mapperMock.Map<ConceptSystem>(conceptSystemCreate).Returns(conceptSystem);
        mapperMock.Map<ConceptSystem>(conceptSystemUpdate).Returns(conceptSystem);

        identityServiceMock.GetCurrentEmail().ReturnsForAnyArgs(CurrentEmail);

        service = new ConceptSystemService(conceptSystemRepositoryMock, collectionRepositoryMock, entryRepositoryMock, identityServiceMock, unitOfWorkMock, mapperMock);
    }

    [Test]
    public async Task GetAllShouldCallRepository()
    {
        var result = await service.GetAll();

        await conceptSystemRepositoryMock.Received().GetAll();
        Assert.That(result, Is.EqualTo(conceptSystemSummaries));
    }

    [Test]
    public async Task GetAllDetailsShouldCallRepository()
    {
        var result = await service.GetAllDetails();

        await conceptSystemRepositoryMock.Received().GetAllDetails();
        Assert.That(result, Is.EqualTo(conceptSystemDetails));
    }

    [Test]
    public async Task GetDetailCallsRepository()
    {
        var result = await service.GetDetail(1);

        Assert.That(conceptSystem.ConceptSystemPositions.Select(p => p.SortIndex), Is.EquivalentTo(Enumerable.Range(0, conceptSystem.ConceptSystemPositions.Count)));
        Assert.That(result, Is.EqualTo(conceptSystemUpdate));
    }

    [Test]
    public void GetDetailThrowsNotEntityFoundException()
    {
        Assert.ThrowsAsync<EntityNotFoundException>(() => service.GetDetail(2));
    }

    [Test]
    public async Task CreateShouldReturnGlossaryDetail()
    {
        var result = await service.Create(conceptSystemCreate);
        await conceptSystemRepositoryMock.Received().Create(conceptSystem);
        await unitOfWorkMock.Received().SaveChangesAsync();
        Assert.That(result, Is.EqualTo(conceptSystemDetail));
        Assert.That(conceptSystem.ConceptSystemPositions.Select(p => p.SortIndex), Is.EquivalentTo(Enumerable.Range(0, conceptSystem.ConceptSystemPositions.Count)));
        Assert.That(conceptSystem.ModifiedBy, Is.EqualTo(CurrentEmail));
        Assert.That(conceptSystem.CreatedBy, Is.EqualTo(CurrentEmail));
    }

    [Test]
    public void CreateShouldThrowWhenCollectionNotFound()
    {
        collectionRepositoryMock.GetById(conceptSystem.CollectionId).ReturnsNull();

        Assert.ThrowsAsync<EntityNotFoundException>(() => service.Create(conceptSystemCreate));
    }

    [Test]
    public void CreateShouldThrowWhenCollectionAlreadyUsed()
    {
        conceptSystem.Collection.ConceptSystem = conceptSystem;

        Assert.ThrowsAsync<BusinessValidationException>(() => service.Create(conceptSystemCreate));
    }

    [Test]
    public async Task UpdateCallsRepository()
    {
        var result = await service.Update(conceptSystemUpdate);
        conceptSystemRepositoryMock.Received().Update(conceptSystem, conceptSystem);
        await unitOfWorkMock.Received().SaveChangesAsync();
        Assert.That(conceptSystem.ConceptSystemPositions.Select(p => p.SortIndex), Is.EquivalentTo(Enumerable.Range(0, conceptSystem.ConceptSystemPositions.Count)));
        Assert.That(result, Is.EqualTo(conceptSystemDetail));
        Assert.That(conceptSystem.ModifiedBy, Is.EqualTo(CurrentEmail));
    }

    [Test]
    public void UpdateShouldThrowWhenConceptSystemNotFound()
    {
        conceptSystemRepositoryMock.GetByIdForEdit(conceptSystem.Id).ReturnsNull();

        Assert.ThrowsAsync<EntityNotFoundException>(() => service.Update(conceptSystemUpdate));
    }

    [Test]
    public void UpdateShouldThrowWhenCollectionNotFound()
    {
        collectionRepositoryMock.GetById(conceptSystem.CollectionId).ReturnsNull();

        Assert.ThrowsAsync<EntityNotFoundException>(() => service.Update(conceptSystemUpdate));
    }

    [Test]
    public void UpdateShouldThrowWhenCollectionAlreadyUsed()
    {
        conceptSystemUpdate.CollectionId = 11;
        collectionRepositoryMock.GetById(11).Returns(new Collection
        {
            ConceptSystem = new()
        });

        Assert.ThrowsAsync<BusinessValidationException>(() => service.Update(conceptSystemUpdate));
    }

    [Test]
    public void UpdateShouldThrowWhenPositionsUsed()
    {
        conceptSystemUpdate.CollectionId = 11;
        collectionRepositoryMock.GetById(11).Returns(new Collection());
        entryRepositoryMock.GetIdsByConceptPositionIds(null).ReturnsForAnyArgs(Enumerable.Range(1, 2));

        Assert.ThrowsAsync<BusinessValidationException>(() => service.Update(conceptSystemUpdate));
    }

    [Test]
    public async Task DeleteCallsRepository()
    {
        await service.Delete(1);
        conceptSystemRepositoryMock.Received().Delete(conceptSystem);
        await unitOfWorkMock.Received().SaveChangesAsync();
    }

    [Test]
    public void DeleteShouldThrowWhenConceptSystemNotFound()
    {
        Assert.ThrowsAsync<EntityNotFoundException>(() => service.Delete(2));
    }

    [Test]
    public void DeleteShouldThrowWhenPositionsUsed()
    {
        entryRepositoryMock.GetIdsByConceptPositionIds(null).ReturnsForAnyArgs(Enumerable.Range(1, 2));

        Assert.ThrowsAsync<BusinessValidationException>(() => service.Delete(1));
    }
}
