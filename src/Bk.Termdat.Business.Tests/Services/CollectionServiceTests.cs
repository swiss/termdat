using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Bk.Termdat.Business.Clients;
using Bk.Termdat.Business.Domain;
using Bk.Termdat.Business.Domain.Projections;
using Bk.Termdat.Business.Mapping;
using Bk.Termdat.Business.Repositories;
using Bk.Termdat.Business.Resource.Models;
using Bk.Termdat.Business.Services;
using Bk.Termdat.Business.Services.Interfaces;
using Bk.Termdat.Common.Resources;
using Bk.Termdat.CrossCutting.Exceptions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ClearExtensions;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using DomainModels = Bk.Termdat.Business.Domain.Models;

namespace Bk.Termdat.Business.Tests.Services;

[TestFixture]
internal class CollectionServiceTests
{
    private CollectionService collectionService;
    private ICollectionRepository collectionRepositoryMock;
    private IMapper mapperMock;
    private readonly IEntryRepository entryRepositoryMock = Substitute.For<IEntryRepository>();
    private readonly IUnitOfWork unitOfWorkMock = Substitute.For<IUnitOfWork>();
    private readonly ISearchQueueClient searchQueueClientMock = Substitute.For<ISearchQueueClient>();
    private readonly IIdentityService identityServiceMock = Substitute.For<IIdentityService>();
    private readonly IAuthorizationService authorizationServiceMock = Substitute.For<IAuthorizationService>();
    private ICollectionValidationService collectionValidationServiceMock;
    private ILanguageDescriptionMapper languageDescriptionMapperMock;
    private IList<DomainModels.Collection> collections;
    private DomainModels.Collection collection;
    private CollectionDetail collectionDetail;
    private CollectionCreate collectionCreate;
    private CollectionUpdate collectionUpdate;
    private const string CurrentEmail = "Its@me.ch";

    [SetUp]
    public void SetUp()
    {
        mapperMock = Substitute.For<IMapper>();
        collectionRepositoryMock = Substitute.For<ICollectionRepository>();
        collectionValidationServiceMock = Substitute.For<ICollectionValidationService>();
        languageDescriptionMapperMock = Substitute.For<ILanguageDescriptionMapper>();
        collections = new List<DomainModels.Collection>
        {
            new()
            {
                Id = 1,
                Active = true,
                ActiveApi = false,
                ClassificationId = null,
                Code = "123",
                Description = "Description",
                Documentation = "Doku"
            },
            new()
            {
                Id = 2,
                Active = false,
                ActiveApi = false,
                ClassificationId = null,
                Code = "abc",
                Description = "Description 2",
                Documentation = "Docu"
            }
        };

        collectionCreate = new()
        {
            Active = true,
            ActiveApi = false,
            ClassificationId = null,
            Code = "123",
            DescriptionDe = "de",
            DescriptionEn = "en",
            DescriptionFr = "fr",
            DescriptionIt = "it",
            DescriptionRm = "rm",
            Description = "description",
            Documentation = "doku"
        };

        collectionUpdate = new()
        {
            Id = 1,
            Active = true,
            ActiveApi = false,
            ClassificationId = null,
            Code = "123",
            DescriptionDe = "de",
            DescriptionEn = "en",
            DescriptionFr = "fr",
            DescriptionIt = "it",
            DescriptionRm = "rm",
            Description = "description",
            Documentation = "doku"
        };

        collection = new()
        {
            Id = 1,
            Active = collectionCreate.Active,
            ActiveApi = collectionCreate.ActiveApi,
            ClassificationId = collectionCreate.ClassificationId,
            Code = collectionCreate.Code,
            Description = collectionCreate.Description,
            Documentation = collectionCreate.Documentation,
            Entries = new List<DomainModels.Entry>
            {
                new() { Id = 10 },
                new() { Id = 11 }
            }
        };

        collectionRepositoryMock.GetById(Arg.Is(1)).Returns(collection);
        collectionRepositoryMock.GetByIdForDelete(Arg.Is(1)).Returns(new CollectionWithEntryCount
        {
            Collection = collection,
            EntryCount = collection.Entries.Count
        });

        collectionDetail = new()
        {
            Id = collection.Id,
            Active = collection.Active,
            ActiveApi = collection.ActiveApi,
            Classification = null,
            Code = collection.Code,
            ConceptSystem = null,
            EntryCount = 0,
            IsObsolete = false,
            Name = null
        };

        mapperMock.Map<DomainModels.Collection>(collectionCreate).Returns(collection);
        collectionValidationServiceMock.ValidateClassification(collectionCreate).Returns(null as DomainModels.Classification);
        languageDescriptionMapperMock.Map<DomainModels.CollectionTranslation>(collectionCreate).Returns(Array.Empty<DomainModels.CollectionTranslation>());
        collectionRepositoryMock.GetById(1).Returns(collection);
        mapperMock.Map<CollectionDetail>(collection).Returns(collectionDetail);
        mapperMock.Map<CollectionUpdate>(collection).Returns(collectionUpdate);

        identityServiceMock.GetCurrentEmail().ReturnsForAnyArgs(CurrentEmail);
        unitOfWorkMock
            .When(uow => uow.ExecuteInTransactionAsync(Arg.Any<Func<Task>>()))
            .Do(async x => await x.Arg<Func<Task>>()());

        collectionService = new CollectionService(
            mapperMock,
            collectionValidationServiceMock,
            collectionRepositoryMock,
            languageDescriptionMapperMock,
            entryRepositoryMock,
            unitOfWorkMock,
            searchQueueClientMock,
            identityServiceMock,
            authorizationServiceMock,
            Substitute.For<ILogger<CollectionService>>()
        );
    }

    [TearDown]
    public void TearDown()
    {
        mapperMock.ClearSubstitute();
        collectionValidationServiceMock.ClearSubstitute();
        collectionRepositoryMock.ClearSubstitute();
        languageDescriptionMapperMock.ClearSubstitute();
        identityServiceMock.ClearSubstitute();
        unitOfWorkMock.ClearSubstitute();
        entryRepositoryMock.ClearSubstitute();
        authorizationServiceMock.ClearSubstitute();
        searchQueueClientMock.ClearSubstitute();
    }

    [Test]
    public async Task GetAllDetails_WhenCalled_ShouldReturnCollectionDetails()
    {
        var collectionsWithEntryCount = collections.Select(c => new CollectionWithEntryCount
        {
            Collection = c,
            EntryCount = c.Id == 1 ? 12 : 0
        });
        collectionRepositoryMock.GetAllDetails().Returns(collectionsWithEntryCount);
        var collectionDetails = new List<CollectionDetail> {
            new()
            {
                Active = collections.First().Active,
                ActiveApi = collections.First().ActiveApi,
                Code = collections.First().Code,
                EntryCount = 12
            },
             new()
             {
                Active = collections.Last().Active,
                ActiveApi = collections.Last().ActiveApi,
                Code = collections.Last().Code,
                EntryCount = 0
            }
        };
        mapperMock.Map<IEnumerable<CollectionDetail>>(collectionsWithEntryCount).Returns(collectionDetails);

        var result = (await collectionService.GetAllDetails()).ToList();
        Assert.That(result, Is.EqualTo(collectionDetails));
    }

    [Test]
    public async Task Create_WithValidCollection_ShouldReturnCollectionDetail()
    {
        var result = await collectionService.Create(collectionCreate);
        await collectionValidationServiceMock.Received().ValidateClassification(collectionCreate);
        await languageDescriptionMapperMock.Received().Map<DomainModels.CollectionTranslation>(collectionCreate);
        await collectionRepositoryMock.Received().Create(collection);
        await unitOfWorkMock.Received().SaveChangesAsync();
        Assert.That(result, Is.EqualTo(collectionDetail));
        Assert.That(collection.ModifiedBy, Is.EqualTo(CurrentEmail));
        Assert.That(collection.CreatedBy, Is.EqualTo(CurrentEmail));
    }

    [Test]
    public async Task Update_NonExistingCollection_ShouldThrowEntityNotFoundException()
    {
        collectionRepositoryMock.GetById(collectionUpdate.Id).Returns(Task.FromResult<DomainModels.Collection>(null));
        await unitOfWorkMock.DidNotReceive().SaveChangesAsync();
        Assert.ThrowsAsync<EntityNotFoundException>(async () => await collectionService.Update(collectionUpdate));
    }

    [Test]
    public async Task Update_ValidCollection_ShouldReturnCollectionDetail()
    {
        var entryIds = new[] { 1, 2 };
        entryRepositoryMock.GetIdsByCollectionId(collection.Id).Returns(entryIds);
        mapperMock.Map<DomainModels.Collection>(collectionUpdate).Returns(collection);
        mapperMock.Map<CollectionDetail>(collection).Returns(collectionDetail);

        var result = await collectionService.Update(collectionUpdate);

        await collectionValidationServiceMock.Received().ValidateClassification(collectionUpdate);
        await languageDescriptionMapperMock.Received().Map<DomainModels.CollectionTranslation>(collectionUpdate);
        collectionRepositoryMock.Received().Update(collection, collection);
        await unitOfWorkMock.Received().SaveChangesAsync();
        await searchQueueClientMock.Received(1).QueueIndexUpdateByIds(Arg.Is(entryIds));

        Assert.That(result, Is.EqualTo(collectionDetail));
        Assert.That(collection.ModifiedBy, Is.EqualTo(CurrentEmail));
    }

    [Test]
    public async Task GetDetail_NonExistingCollection_ShouldThrowEntityNotFoundException()
    {
        collectionRepositoryMock.GetById(0).Returns(Task.FromResult<DomainModels.Collection>(null));
        Assert.ThrowsAsync<EntityNotFoundException>(async () => await collectionService.GetDetail(0));

        await collectionRepositoryMock.Received().GetById(0);
    }

    [Test]
    public async Task GetDetail_ValidCollection_ShouldReturnCollectionUpdate()
    {
        var result = await collectionService.GetDetail(1);

        Assert.That(result, Is.EqualTo(collectionUpdate));

        await collectionRepositoryMock.Received().GetById(1);
    }

    [Test]
    public async Task GetAll_WhenCalled_ReturnsAllCollections()
    {
        collectionRepositoryMock.GetAll().Returns(collections);
        var mappedCollections = new List<CollectionSummary>();
        mapperMock.Map<IEnumerable<CollectionSummary>>(collections).Returns(mappedCollections);

        var result = await collectionService.GetAll();

        Assert.That(result, Is.EqualTo(mappedCollections));
    }

    [Test]
    public async Task GetAllActive_WhenCalled_ShouldCallRepository()
    {
        collectionRepositoryMock.GetAllActive().Returns(collections);
        var mappedCollections = new List<CollectionSummary>();
        mapperMock.Map<IEnumerable<CollectionSummary>>(collections).Returns(mappedCollections);

        var result = await collectionService.GetAllActive();

        Assert.That(result, Is.EqualTo(mappedCollections));
    }

    [Test]
    public async Task GetAllForCurrentUser_WhenCalled_ShouldCallRepository()
    {
        collectionRepositoryMock.GetAccessibleActiveAndInactive(Arg.Any<IEnumerable<int>>()).Returns(collections);
        var mappedCollections = new List<CollectionSummary>();
        mapperMock.Map<IEnumerable<CollectionSummary>>(collections).Returns(mappedCollections);

        var result = await collectionService.GetAllForCurrentUser();

        Assert.That(result, Is.EqualTo(mappedCollections));
    }

    [Test]
    public async Task GetById_NonExistingCollection_ReturnsNullWhenEntryIsNotFound()
    {
        var result = await collectionService.GetById(2);

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetById_ExistingCollection_ReturnsCorrectCollectionDetail()
    {
        var collectionDetailMock = new CollectionDetail { Id = 1 };
        mapperMock.Map<CollectionDetail>(collection).Returns(collectionDetailMock);

        var result = await collectionService.GetById(1);

        Assert.That(result, Is.InstanceOf<CollectionDetail>());
        Assert.That(result, Is.EqualTo(collectionDetailMock));
    }

    [Test]
    public void Delete_NonExistingCollection_ThrowsException()
    {
        var exceptionSoftDelete = Assert.ThrowsAsync<EntityNotFoundException>(() => collectionService.Delete(2));
        Assert.That(exceptionSoftDelete, Is.Not.Null);
        Assert.That(exceptionSoftDelete.Message, Is.EqualTo(string.Format(CultureInfo.InvariantCulture, BackendTexts.CollectionNotFound, 2)));
        var exceptionDelete = Assert.ThrowsAsync<EntityNotFoundException>(() => collectionService.Delete(2));
        Assert.That(exceptionDelete, Is.Not.Null);
        Assert.That(exceptionDelete.Message, Is.EqualTo(string.Format(CultureInfo.InvariantCulture, BackendTexts.CollectionNotFound, 2)));
    }

    [Test]
    public void Restore_NonExistingCollection_ThrowsException()
    {
        var exception = Assert.ThrowsAsync<EntityNotFoundException>(() => collectionService.Restore(2));
        Assert.That(exception, Is.Not.Null);
        Assert.That(exception.Message, Is.EqualTo(string.Format(CultureInfo.InvariantCulture, BackendTexts.CollectionNotFound, 2)));
    }

    [Test]
    public async Task Delete_ExistingCollection_CanSoftDeleteCollection()
    {
        var entryIdsForCollection = Enumerable.Range(3, 10).ToArray();
        entryRepositoryMock.GetIdsByCollectionId(1).Returns(entryIdsForCollection);

        await collectionService.Delete(1, soft: true);

        identityServiceMock.Received(1).GetCurrentEmail();
        await collectionRepositoryMock.Received(1).UpdateDeletedFlag(Arg.Is(collection), Arg.Is(true), Arg.Is(CurrentEmail));
        await unitOfWorkMock.Received(1).SaveChangesAsync();
        await searchQueueClientMock.Received(1).QueueIndexUpdateByIds(Arg.Is(entryIdsForCollection));
    }

    [Test]
    public async Task Delete_SoftDeletedCollectionWithoutEntries_DeletesCollection()
    {
        collection.Deleted = true;
        collectionRepositoryMock.GetByIdForDelete(Arg.Is(1)).Returns(new CollectionWithEntryCount
        {
            Collection = collection,
            EntryCount = 0
        });

        await collectionService.Delete(1, soft: false);

        await collectionRepositoryMock.DidNotReceiveWithAnyArgs().UpdateDeletedFlag(Arg.Any<DomainModels.Collection>(), Arg.Any<bool>(), Arg.Any<string>());
        collectionRepositoryMock.Received(1).Delete(Arg.Is(collection));
        await unitOfWorkMock.Received(1).SaveChangesAsync();
        await searchQueueClientMock.DidNotReceiveWithAnyArgs().QueueIndexUpdateByIds(Arg.Any<IEnumerable<int>>());
    }

    [Test]
    public async Task Delete_SoftDeletedCollectionWithEntries_ThrowsException()
    {
        collection.Deleted = true;
        collectionRepositoryMock.GetByIdForDelete(Arg.Is(1)).Returns(new CollectionWithEntryCount
        {
            Collection = collection,
            EntryCount = 1
        });

        var exception = Assert.ThrowsAsync<BusinessValidationException>(() => collectionService.Delete(1, soft: false));
        Assert.That(exception, Is.Not.Null);
        Assert.That(exception.ModelKey, Is.EqualTo(nameof(DomainModels.Collection)));
        Assert.That(exception.Message, Is.EqualTo(BackendTexts.CollectionDeleteFailedHasEntries));
    }

    [Test]
    public async Task Delete_NotSoftDeletedCollection_ThrowsException()
    {
        collectionRepositoryMock.GetByIdForDelete(Arg.Is(1)).Returns(new CollectionWithEntryCount
        {
            Collection = collection,
            EntryCount = 0
        });

        var exception = Assert.ThrowsAsync<BusinessValidationException>(() => collectionService.Delete(1, soft: false));
        Assert.That(exception, Is.Not.Null);
        Assert.That(exception.ModelKey, Is.EqualTo(nameof(DomainModels.Collection)));
        Assert.That(exception.Message, Is.EqualTo(BackendTexts.CollectionDeleteFailedNotSoftDeleted));
    }

    [Test]
    public void SoftDeleteThrowsNoExceptionWhenQueueIndexUpdateByCollectionFails()
    {
        searchQueueClientMock.QueueIndexUpdateByIds(Arg.Any<IEnumerable<int>>()).ThrowsForAnyArgs(new ArgumentException("SHOULD NOT THROW"));

        Assert.DoesNotThrowAsync(() => collectionService.Delete(1));
    }

    [Test]
    public async Task RestoreCanUndoSoftDeleteOfCollection()
    {
        var entryIdsForCollection = Enumerable.Range(3, 10).ToArray();
        entryRepositoryMock.GetIdsByCollectionId(1).Returns(entryIdsForCollection);

        await collectionService.Restore(1);

        identityServiceMock.Received(1).GetCurrentEmail();
        await collectionRepositoryMock.Received(1).UpdateDeletedFlag(Arg.Is(collection), Arg.Is(false), Arg.Is(CurrentEmail));
        await unitOfWorkMock.Received(1).SaveChangesAsync();
        await searchQueueClientMock.Received(1).QueueIndexUpdateByIds(Arg.Is(entryIdsForCollection));
    }

    [Test]
    public void RestoreThrowsNoExceptionWhenQueueIndexUpdateByCollectionFails()
    {
        searchQueueClientMock.QueueIndexUpdateByIds(Arg.Any<IEnumerable<int>>()).ThrowsForAnyArgs(new ArgumentException("SHOULD NOT THROW"));

        Assert.DoesNotThrowAsync(() => collectionService.Restore(1));
    }

    [Test]
    public async Task GetAllForModification_ForWriter_ReturnsOnlyAuthorized()
    {
        var authorizedIds = new[] { 1, 2 };
        authorizationServiceMock.IsWriter().Returns(true);
        authorizationServiceMock.GetAuthorizedCollectionIds().Returns(authorizedIds);
        collectionRepositoryMock.GetAuthorizedByIds(Arg.Any<IEnumerable<int>>()).Returns(collections);

        var mappedCollections = new List<CollectionSummary>();
        mapperMock.Map<IEnumerable<CollectionSummary>>(collections).Returns(mappedCollections);

        var result = await collectionService.GetAllForModification();
        Assert.That(result, Is.EqualTo(mappedCollections));

        authorizationServiceMock.Received(1).GetAuthorizedCollectionIds();
        await collectionRepositoryMock.Received(1).GetAuthorizedByIds(authorizedIds);
    }

    [Test]
    public async Task GetAllForModification_ForOtherRoles_ReturnsOnlyAuthorized()
    {
        authorizationServiceMock.IsWriter().Returns(false);
        collectionRepositoryMock.GetAll().Returns(collections);

        var mappedCollections = new List<CollectionSummary>();
        mapperMock.Map<IEnumerable<CollectionSummary>>(collections).Returns(mappedCollections);

        var result = await collectionService.GetAllForModification();
        Assert.That(result, Is.EqualTo(mappedCollections));

        authorizationServiceMock.Received(0).GetAuthorizedCollectionIds();
        await collectionRepositoryMock.Received(0).GetAuthorizedByIds(Arg.Any<IEnumerable<int>>());
    }
}
