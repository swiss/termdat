using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Bk.Termdat.Business.Clients;
using Bk.Termdat.Business.Domain;
using Bk.Termdat.Business.Domain.Models;
using Bk.Termdat.Business.Domain.Projections;
using Bk.Termdat.Business.Mapping;
using Bk.Termdat.Business.Repositories;
using Bk.Termdat.Business.Resource.Models;
using Bk.Termdat.Business.Services.Interfaces;
using Bk.Termdat.Common.Resources;
using Bk.Termdat.CrossCutting.Exceptions;
using Microsoft.Extensions.Logging;

namespace Bk.Termdat.Business.Services;

public class CollectionService : ICollectionService
{
    private readonly IMapper mapper;
    private readonly ICollectionRepository collectionRepository;
    private readonly IEntryRepository entryRepository;
    private readonly ISearchQueueClient searchQueueClient;
    private readonly ILogger<CollectionService> logger;
    private readonly ILanguageDescriptionMapper languageDescriptionMapper;
    private readonly ICollectionValidationService collectionValidationService;
    private readonly IIdentityService identityService;
    private readonly IAuthorizationService authorizationService;
    private readonly IUnitOfWork unitOfWork;

#pragma warning disable S107
    public CollectionService(
        IMapper mapper,
        ICollectionValidationService collectionValidationService,
        ICollectionRepository collectionRepository,
        ILanguageDescriptionMapper languageDescriptionMapper,
        IEntryRepository entryRepository,
        IUnitOfWork unitOfWork,
        ISearchQueueClient searchQueueClient,
        IIdentityService identityService,
        IAuthorizationService authorizationService,
        ILogger<CollectionService> logger)
#pragma warning restore S107
    {
        this.unitOfWork = unitOfWork;
        this.mapper = mapper;
        this.identityService = identityService;
        this.authorizationService = authorizationService;
        this.languageDescriptionMapper = languageDescriptionMapper;
        this.collectionValidationService = collectionValidationService;
        this.collectionRepository = collectionRepository;
        this.entryRepository = entryRepository;
        this.unitOfWork = unitOfWork;
        this.searchQueueClient = searchQueueClient;
        this.identityService = identityService;
        this.logger = logger;
    }

    public async Task<IEnumerable<CollectionSummary>> GetAll()
    {
        var collections = await collectionRepository.GetAll();
        return mapper.Map<IEnumerable<CollectionSummary>>(collections);
    }

    public async Task<IEnumerable<CollectionSummary>> GetAllActive()
    {
        var collections = await collectionRepository.GetAllActive();
        return mapper.Map<IEnumerable<CollectionSummary>>(collections);
    }

    public async Task<IEnumerable<CollectionSummary>> GetAllForCurrentUser()
    {
        var collections = await collectionRepository.GetAccessibleActiveAndInactive(authorizationService.GetAuthorizedCollectionIds());

        return mapper.Map<IEnumerable<CollectionSummary>>(collections);
    }

    public async Task<IEnumerable<CollectionSummary>> GetAllForModification()
    {
        var collections = authorizationService.IsWriter() ?
            await collectionRepository.GetAuthorizedByIds(authorizationService.GetAuthorizedCollectionIds()) :
            await collectionRepository.GetAllActive();

        return mapper.Map<IEnumerable<CollectionSummary>>(collections);
    }

    public async Task<IEnumerable<CollectionDetail>> GetAllDetails()
    {
        var collections = await collectionRepository.GetAllDetails();
        return mapper.Map<IEnumerable<CollectionDetail>>(collections);
    }

    public async Task<CollectionDetail> Create(CollectionCreate collectionCreate)
    {
        var mappedCollection = mapper.Map<Collection>(collectionCreate);
        mappedCollection.Modified = DateTime.UtcNow;
        mappedCollection.ModifiedBy = identityService.GetCurrentEmail();
        mappedCollection.Created = DateTime.UtcNow;
        mappedCollection.CreatedBy = identityService.GetCurrentEmail();

        mappedCollection.Classification = await collectionValidationService.ValidateClassification(collectionCreate);
        mappedCollection.Translations = (await languageDescriptionMapper.Map<CollectionTranslation>(collectionCreate)).ToList();

        await collectionRepository.Create(mappedCollection);

        await unitOfWork.SaveChangesAsync();

        var collection = await collectionRepository.GetById(mappedCollection.Id);
        return mapper.Map<CollectionDetail>(collection);
    }

    public async Task<CollectionDetail> Update(CollectionUpdate collectionUpdate)
    {
        ArgumentNullException.ThrowIfNull(collectionUpdate);

        var databaseCollection = await collectionRepository.GetById(collectionUpdate.Id);

        if (databaseCollection == null)
        {
            throw new EntityNotFoundException(string.Format(CultureInfo.InvariantCulture, BackendTexts.CollectionNotFound, collectionUpdate.Id));
        }

        var mappedCollection = mapper.Map<Collection>(collectionUpdate);
        mappedCollection.Modified = DateTime.UtcNow;
        mappedCollection.ModifiedBy = identityService.GetCurrentEmail();
        mappedCollection.Created = databaseCollection.Created;
        mappedCollection.CreatedBy = databaseCollection.CreatedBy;
        mappedCollection.Deleted = databaseCollection.Deleted;

        mappedCollection.Classification = await collectionValidationService.ValidateClassification(collectionUpdate);
        mappedCollection.Translations = (await languageDescriptionMapper.Map<CollectionTranslation>(collectionUpdate)).ToList();

        collectionRepository.Update(databaseCollection, mappedCollection);

        await unitOfWork.SaveChangesAsync();

        var collection = await collectionRepository.GetById(databaseCollection.Id);
        var collectionDetail = mapper.Map<CollectionDetail>(collection);

        await UpdateSearchIndex();

        return collectionDetail;

        async Task UpdateSearchIndex()
        {
            try
            {
                var entryIds = await entryRepository.GetIdsByCollectionId(collection.Id);
                await searchQueueClient.QueueIndexUpdateByIds(entryIds);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Error while updating entries in search index after collection '{Id}' was updated", collection.Id);
            }
        }
    }

    public async Task<CollectionDetail> GetById(int id)
    {
        var collection = await collectionRepository.GetById(id);

        return collection == null ? null : mapper.Map<CollectionDetail>(collection);
    }

    public async Task Delete(int id, bool soft = true)
    {
        var collectionWithEntryCount = await GetCollectionForDelete(id);

        if (soft)
        {
            await UpdateDeletedFlag(collectionWithEntryCount.Collection);
        }
        else
        {
            await Delete(collectionWithEntryCount);
        }
    }

    public async Task Restore(int id)
    {
        var collectionWithEntryCount = await GetCollectionForDelete(id);

        await UpdateDeletedFlag(collectionWithEntryCount.Collection, false);
    }

    public async Task<CollectionUpdate> GetDetail(int id)
    {
        var collection = await collectionRepository.GetById(id);
        if (collection == null)
        {
            throw new EntityNotFoundException(string.Format(CultureInfo.InvariantCulture, BackendTexts.CollectionNotFound, id));
        }
        var mappedCollection = mapper.Map<CollectionUpdate>(collection);

        return mappedCollection;
    }

    private async Task<CollectionWithEntryCount> GetCollectionForDelete(int id)
    {
        var collectionWithEntryCount = await collectionRepository.GetByIdForDelete(id);
        if (collectionWithEntryCount == null)
        {
            throw new EntityNotFoundException(string.Format(CultureInfo.InvariantCulture, BackendTexts.CollectionNotFound, id));
        }

        return collectionWithEntryCount;
    }

    private async Task UpdateDeletedFlag(Collection collection, bool delete = true)
    {
        var entriesIdsWithCollection = await entryRepository.GetIdsByCollectionId(collection.Id);

        await unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            var modifiedBy = identityService.GetCurrentEmail();

            await collectionRepository.UpdateDeletedFlag(collection, delete, modifiedBy);

            await unitOfWork.SaveChangesAsync();
        });

        try
        {
            await searchQueueClient.QueueIndexUpdateByIds(entriesIdsWithCollection);
        }
        catch (Exception exception)
        {
            var action = delete ? "soft deleted" : "restored";
            logger.LogError(exception, "Error while updating entries in search index after collection '{Id}' was {Action}", collection.Id, action);
        }
    }

    private async Task Delete(CollectionWithEntryCount collectionWithEntryCount)
    {
        if (!collectionWithEntryCount.Collection.Deleted)
        {
            throw new BusinessValidationException(nameof(Collection), BackendTexts.CollectionDeleteFailedNotSoftDeleted);
        }

        if (collectionWithEntryCount.EntryCount != 0)
        {
            throw new BusinessValidationException(nameof(Collection), BackendTexts.CollectionDeleteFailedHasEntries);
        }

        collectionRepository.Delete(collectionWithEntryCount.Collection);
        await unitOfWork.SaveChangesAsync();
    }
}
