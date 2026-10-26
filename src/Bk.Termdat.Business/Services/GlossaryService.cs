using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Bk.Termdat.Business.Clients;
using Bk.Termdat.Business.Domain;
using Bk.Termdat.Business.Domain.Models;
using Bk.Termdat.Business.Mapping;
using Bk.Termdat.Business.Repositories;
using Bk.Termdat.Business.Resource.Models;
using Bk.Termdat.Business.Services.Interfaces;
using Bk.Termdat.Common.Resources;
using Bk.Termdat.CrossCutting.Exceptions;
using Microsoft.Extensions.Logging;

namespace Bk.Termdat.Business.Services;

public class GlossaryService : IGlossaryService
{
    private readonly IGlossaryRepository glossaryRepository;
    private readonly IEntryRepository entryRepository;
    private readonly IIdentityService identityService;
    private readonly ISearchQueueClient searchQueueClient;
    private readonly IUnitOfWork unitOfWork;
    private readonly ILanguageDescriptionMapper languageDescriptionMapper;
    private readonly IMapper mapper;
    private readonly IAuthorizationService authorizationService;
    private readonly ILogger<GlossaryService> logger;

#pragma warning disable S107
    public GlossaryService(
        IGlossaryRepository glossaryRepository,
        IEntryRepository entryRepository,
        IIdentityService identityService,
        ISearchQueueClient searchQueueClient,
        IUnitOfWork unitOfWork,
        ILanguageDescriptionMapper languageDescriptionMapper,
        IMapper mapper,
        IAuthorizationService authorizationService,
        ILogger<GlossaryService> logger)
#pragma warning restore S107
    {
        this.glossaryRepository = glossaryRepository;
        this.entryRepository = entryRepository;
        this.identityService = identityService;
        this.searchQueueClient = searchQueueClient;
        this.unitOfWork = unitOfWork;
        this.languageDescriptionMapper = languageDescriptionMapper;
        this.mapper = mapper;
        this.authorizationService = authorizationService;
        this.logger = logger;
    }

    public async Task<IEnumerable<GlossarySummary>> GetAll()
    {
        var glossaries = await glossaryRepository.GetAll();

        return mapper.Map<IEnumerable<GlossarySummary>>(glossaries);
    }

    public async Task<IEnumerable<GlossarySummary>> GetAllForModification()
    {
        var glossaries = authorizationService.IsWriter() ?
            await glossaryRepository.GetAuthorizedByIds(authorizationService.GetAuthorizedGlossaryIds()) :
            await glossaryRepository.GetAll();

        return mapper.Map<IEnumerable<GlossarySummary>>(glossaries);
    }

    public async Task<IEnumerable<GlossaryDetail>> GetAllDetails()
    {
        var glossaries = await glossaryRepository.GetAllDetails();

        return mapper.Map<IEnumerable<GlossaryDetail>>(glossaries);
    }

    public async Task<GlossaryUpdate> GetDetail(int id)
    {
        var glossary = await glossaryRepository.GetById(id);
        if (glossary == null)
        {
            throw new EntityNotFoundException(string.Format(CultureInfo.InvariantCulture, BackendTexts.GlossaryNotFound, id));
        }
        var mappedGlossary = mapper.Map<GlossaryUpdate>(glossary);

        return mappedGlossary;
    }

    public async Task<GlossaryDetail> Create(GlossaryCreate glossaryCreate)
    {
        var mappedGlossary = mapper.Map<Glossary>(glossaryCreate);
        mappedGlossary.Modified = DateTime.UtcNow;
        mappedGlossary.ModifiedBy = identityService.GetCurrentEmail();
        mappedGlossary.Created = DateTime.UtcNow;
        mappedGlossary.CreatedBy = identityService.GetCurrentEmail();

        mappedGlossary.Translations = (await languageDescriptionMapper.Map<GlossaryTranslation>(glossaryCreate)).ToList();

        await glossaryRepository.Create(mappedGlossary);

        await unitOfWork.SaveChangesAsync();

        var glossary = await glossaryRepository.GetById(mappedGlossary.Id);
        return mapper.Map<GlossaryDetail>(glossary);
    }

    public async Task<GlossaryDetail> Update(GlossaryUpdate glossaryUpdate)
    {
        ArgumentNullException.ThrowIfNull(glossaryUpdate);

        var databaseGlossary = await glossaryRepository.GetById(glossaryUpdate.Id);

        if (databaseGlossary == null)
        {
            throw new EntityNotFoundException(string.Format(CultureInfo.InvariantCulture, BackendTexts.GlossaryNotFound, glossaryUpdate.Id));
        }

        var mappedGlossary = mapper.Map<Glossary>(glossaryUpdate);
        mappedGlossary.Modified = DateTime.UtcNow;
        mappedGlossary.ModifiedBy = identityService.GetCurrentEmail();
        mappedGlossary.Created = databaseGlossary.Created;
        mappedGlossary.CreatedBy = databaseGlossary.CreatedBy;

        mappedGlossary.Translations = (await languageDescriptionMapper.Map<GlossaryTranslation>(glossaryUpdate)).ToList();

        glossaryRepository.Update(databaseGlossary, mappedGlossary);

        await unitOfWork.SaveChangesAsync();

        var glossary = await glossaryRepository.GetById(databaseGlossary.Id);
        return mapper.Map<GlossaryDetail>(glossary);
    }

    public async Task Delete(int id)
    {
        var glossary = await glossaryRepository.GetById(id);

        if (glossary == null)
        {
            throw new EntityNotFoundException(string.Format(CultureInfo.InvariantCulture, BackendTexts.GlossaryNotFound, id));
        }

        var entriesIdsWithGlossary = await entryRepository.GetIdsByGlossaryId(glossary.Id);

        glossaryRepository.Delete(glossary);

        await unitOfWork.SaveChangesAsync();

        try
        {
            await searchQueueClient.QueueIndexUpdateByIds(entriesIdsWithGlossary);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Error while updating entries in search index after glossary '{Id}' was deleted", glossary.Id);
        }
    }
}
