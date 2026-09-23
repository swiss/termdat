using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Bk.Termdat.Business.Domain;
using Bk.Termdat.Business.Domain.Models;
using Bk.Termdat.Business.Repositories;
using Bk.Termdat.Business.Resource.Models;
using Bk.Termdat.Business.Services.Interfaces;
using Bk.Termdat.Common.Resources;
using Bk.Termdat.CrossCutting.Exceptions;

namespace Bk.Termdat.Business.Services;

public class ConceptSystemService : IConceptSystemService
{
    private readonly IConceptSystemRepository conceptSystemRepository;
    private readonly ICollectionRepository collectionRepository;
    private readonly IEntryRepository entryRepository;
    private readonly IIdentityService identityService;
    private readonly IUnitOfWork unitOfWork;
    private readonly IMapper mapper;

    public ConceptSystemService(
        IConceptSystemRepository conceptSystemRepository,
        ICollectionRepository collectionRepository,
        IEntryRepository entryRepository,
        IIdentityService identityService,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        this.conceptSystemRepository = conceptSystemRepository;
        this.collectionRepository = collectionRepository;
        this.entryRepository = entryRepository;
        this.identityService = identityService;
        this.unitOfWork = unitOfWork;
        this.mapper = mapper;
    }

    public async Task<IEnumerable<ConceptSystemSummary>> GetAll()
    {
        var conceptSystems = await conceptSystemRepository.GetAll();
        return mapper.Map<IEnumerable<ConceptSystemSummary>>(conceptSystems);
    }

    public async Task<IEnumerable<ConceptSystemDetail>> GetAllDetails()
    {
        var conceptSystemsDetails = await conceptSystemRepository.GetAllDetails();
        return mapper.Map<IEnumerable<ConceptSystemDetail>>(conceptSystemsDetails);
    }

    public async Task<ConceptSystemUpdate> GetDetail(int id)
    {
        var conceptSystem = await conceptSystemRepository.GetByIdForEdit(id);
        if (conceptSystem == null)
        {
            throw new EntityNotFoundException(string.Format(BackendTexts.ConceptSystemNotFound, id));
        }

        conceptSystem.ConceptSystemPositions = conceptSystem.ConceptSystemPositions
                .OrderBy(p => p.SortIndex)
                .ToList();

        var mappedConceptSystem = mapper.Map<ConceptSystemUpdate>(conceptSystem);

        return mappedConceptSystem;
    }

    public async Task<ConceptSystemDetail> Create(ConceptSystemCreate conceptSystemCreate)
    {
        var mappedConceptSystem = mapper.Map<ConceptSystem>(conceptSystemCreate);
        mappedConceptSystem.Modified = DateTime.UtcNow;
        mappedConceptSystem.ModifiedBy = identityService.GetCurrentEmail();
        mappedConceptSystem.Created = DateTime.UtcNow;
        mappedConceptSystem.CreatedBy = identityService.GetCurrentEmail();

        var collection = await collectionRepository.GetById(conceptSystemCreate.CollectionId);
        if (collection == null)
        {
            throw new EntityNotFoundException(string.Format(BackendTexts.CollectionNotFound, conceptSystemCreate.CollectionId));
        }

        if (collection.ConceptSystem != null)
        {
            throw new BusinessValidationException(nameof(conceptSystemCreate.CollectionId),
                string.Format(BackendTexts.ConceptSystemCollectionAlreadyUsed, collection.Id));
        }

        UpdateSortIndex(mappedConceptSystem);

        mappedConceptSystem.Collection = collection;

        await conceptSystemRepository.Create(mappedConceptSystem);

        await unitOfWork.SaveChangesAsync();

        var conceptSystem = await conceptSystemRepository.GetByIdForEdit(mappedConceptSystem.Id);
        return mapper.Map<ConceptSystemDetail>(conceptSystem);
    }

    public async Task<ConceptSystemDetail> Update(ConceptSystemUpdate conceptSystemUpdate)
    {
        var databaseConceptSystem = await conceptSystemRepository.GetByIdForEdit(conceptSystemUpdate.Id);

        if (databaseConceptSystem == null)
        {
            throw new EntityNotFoundException(string.Format(BackendTexts.ConceptSystemNotFound, conceptSystemUpdate.Id));
        }

        var collection = await collectionRepository.GetById(conceptSystemUpdate.CollectionId);
        if (collection == null)
        {
            throw new EntityNotFoundException(string.Format(BackendTexts.CollectionNotFound, conceptSystemUpdate.CollectionId));
        }

        var changeCollection = collection.Id != databaseConceptSystem.CollectionId;
        if (collection.ConceptSystem != null && changeCollection)
        {
            throw new BusinessValidationException(nameof(conceptSystemUpdate.CollectionId),
                string.Format(BackendTexts.ConceptSystemCollectionAlreadyUsed, collection.Id));
        }

        if (changeCollection)
        {
            await ThrowIfPositionsInUse(databaseConceptSystem, BackendTexts.ConceptSystemUpdateCollectionNotAllowed);
        }

        var mappedConceptSystem = mapper.Map<ConceptSystem>(conceptSystemUpdate);
        mappedConceptSystem.Modified = DateTime.UtcNow;
        mappedConceptSystem.ModifiedBy = identityService.GetCurrentEmail();
        mappedConceptSystem.Created = databaseConceptSystem.Created;
        mappedConceptSystem.CreatedBy = databaseConceptSystem.CreatedBy;
        mappedConceptSystem.Collection = collection;

        UpdateSortIndex(mappedConceptSystem);

        conceptSystemRepository.Update(databaseConceptSystem, mappedConceptSystem);

        await unitOfWork.SaveChangesAsync();

        var conceptSystem = await conceptSystemRepository.GetByIdForEdit(databaseConceptSystem.Id);
        return mapper.Map<ConceptSystemDetail>(conceptSystem);
    }

    private async Task ThrowIfPositionsInUse(ConceptSystem conceptSystem, string message)
    {
        var positionIds = conceptSystem.ConceptSystemPositions.Select(csp => csp.Id);
        var entryIdsWithPositions = (await entryRepository.GetIdsByConceptPositionIds(positionIds)).ToList();
        if (entryIdsWithPositions.Any())
        {
            throw new BusinessValidationException(nameof(ConceptSystemUpdate.CollectionId),
                string.Format(
                    message,
                    string.Join(',', entryIdsWithPositions)));
        }
    }

    private static void UpdateSortIndex(ConceptSystem mappedConceptSystem)
    {
        var sortIndex = default(int);
        foreach (var conceptSystemPosition in mappedConceptSystem.ConceptSystemPositions)
        {
            conceptSystemPosition.SortIndex = sortIndex++;
        }
    }

    public async Task Delete(int id)
    {
        var conceptSystem = await conceptSystemRepository.GetByIdForEdit(id);

        if (conceptSystem == null)
        {
            throw new EntityNotFoundException(string.Format(BackendTexts.ConceptSystemNotFound, id));
        }

        await ThrowIfPositionsInUse(conceptSystem, BackendTexts.ConceptSystemDeleteNotAllowed);

        conceptSystemRepository.Delete(conceptSystem);
        await unitOfWork.SaveChangesAsync();
    }
}
