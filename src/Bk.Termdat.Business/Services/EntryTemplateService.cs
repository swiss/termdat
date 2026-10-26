using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
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

public class EntryTemplateService : IEntryTemplateService
{
    private readonly ICollectionRepository collectionRepository;
    private readonly IEntryValidationService entryValidationService;
    private readonly IIdentityService identityService;
    private readonly JsonSerializerOptions jsonSerializerOptions;
    private readonly IMapper mapper;
    private readonly IOfficeRepository officeRepository;
    private readonly IEntryTemplateRepository repository;
    private readonly IUnitOfWork unitOfWork;
    private readonly IUserAccountRepository userAccountRepository;

    public EntryTemplateService(IEntryValidationService entryValidationService,
        IEntryTemplateRepository repository,
        IMapper mapper,
        IIdentityService identityService,
        IUserAccountRepository userAccountRepository,
        IOfficeRepository officeRepository,
        ICollectionRepository collectionRepository,
        IUnitOfWork unitOfWork
    )
    {
        this.entryValidationService = entryValidationService;
        this.repository = repository;
        this.mapper = mapper;
        this.identityService = identityService;
        this.userAccountRepository = userAccountRepository;
        this.officeRepository = officeRepository;
        this.collectionRepository = collectionRepository;
        this.unitOfWork = unitOfWork;

        jsonSerializerOptions = new JsonSerializerOptions
        {
            ReferenceHandler = ReferenceHandler.Preserve,
            WriteIndented = true
        };
    }

    public async Task<IEnumerable<EntryTemplateSummary>> GetForUser()
    {
        var email = identityService.GetCurrentEmail();
        var user = await userAccountRepository.GetByEmail(email);

        if (user == null)
        {
            throw new EntityNotFoundException($"User with email '{email}' was not found");
        }

        var entryTemplates = await repository.GetForUser(user.Id);
        return mapper.Map<IEnumerable<EntryTemplateSummary>>(entryTemplates);
    }

    public async Task<EntryModificationDetail> GetTemplateById(int id)
    {
        var entryTemplate = await repository.GetById(id);

        var entry = JsonSerializer.Deserialize<Entry>(entryTemplate.Template, jsonSerializerOptions);
        entry.Office = await officeRepository.GetById(entry.OfficeId);
        entry.Collection = await collectionRepository.GetById(entry.CollectionId);

        var entryModificationDetail = mapper.Map<EntryModificationDetail>(entry);

        return entryModificationDetail;
    }

    public async Task Create(EntryTemplateCreate entryTemplateCreate)
    {
        ArgumentNullException.ThrowIfNull(entryTemplateCreate);

        var email = identityService.GetCurrentEmail();
        var user = await userAccountRepository.GetByEmail(email);

        if (user == null)
        {
            throw new EntityNotFoundException($"User with email '{email}' not found");
        }

        if (await repository.TemplateNameExistsForUser(user.Id, entryTemplateCreate.TemplateName))
        {
            throw new BusinessValidationException(
                nameof(EntryTemplateCreate.TemplateName),
                string.Format(CultureInfo.InvariantCulture, BackendTexts.TemplateExistsForUser, entryTemplateCreate.TemplateName));
        }

        var entry = mapper.Map<Entry>(entryTemplateCreate);
        entry.RelatedCollections = await entryValidationService.ValidateRelatedCollections(entryTemplateCreate);
        entry.LenochClassifications = null;
        entry.Subjects = await entryValidationService.ValidateSubjects(entryTemplateCreate);
        entry.Domains = await entryValidationService.ValidateDomains(entryTemplateCreate);
        entry.Glossaries = await entryValidationService.ValidateGlossaries(entryTemplateCreate);
        entry.References = await entryValidationService.ValidateReferences(entryTemplateCreate, entry);
        entry.AdminInfos = await entryValidationService.ValidateAdminInfos(entryTemplateCreate);

        var utcNow = DateTime.UtcNow;
        var entryTemplate = new EntryTemplate
        {
            Name = entryTemplateCreate.TemplateName,
            Template = JsonSerializer.Serialize(entry, jsonSerializerOptions),
            UserAccountId = user.Id,
            Created = utcNow,
            CreatedBy = email,
            Modified = utcNow,
            ModifiedBy = email
        };

        await repository.Create(entryTemplate);

        await unitOfWork.SaveChangesAsync();
    }

    public async Task Update(EntryTemplateUpdate entryTemplateUpdate)
    {
        ArgumentNullException.ThrowIfNull(entryTemplateUpdate);

        var existingTemplate = await repository.GetById(entryTemplateUpdate.Id);

        if (existingTemplate == null)
        {
            throw new EntityNotFoundException(string.Format(CultureInfo.InvariantCulture, BackendTexts.EntryTemplateNotFound, entryTemplateUpdate.Id));
        }

        var entry = mapper.Map<Entry>(entryTemplateUpdate);
        entry.RelatedCollections = await entryValidationService.ValidateRelatedCollections(entryTemplateUpdate);
        entry.LenochClassifications = null;
        entry.Subjects = await entryValidationService.ValidateSubjects(entryTemplateUpdate);
        entry.Domains = await entryValidationService.ValidateDomains(entryTemplateUpdate);
        entry.Glossaries = await entryValidationService.ValidateGlossaries(entryTemplateUpdate);
        entry.References = await entryValidationService.ValidateReferences(entryTemplateUpdate, entry);
        entry.AdminInfos = await entryValidationService.ValidateAdminInfos(entryTemplateUpdate);

        existingTemplate.Template = JsonSerializer.Serialize(entry, jsonSerializerOptions);
        existingTemplate.Modified = DateTime.UtcNow;
        existingTemplate.ModifiedBy = identityService.GetCurrentEmail();

        repository.Update(existingTemplate);

        await unitOfWork.SaveChangesAsync();
    }

    public async Task Delete(int id)
    {
        var entryTemplate = await repository.GetById(id);

        if (entryTemplate == null)
        {
            throw new EntityNotFoundException(string.Format(CultureInfo.InvariantCulture, BackendTexts.EntryTemplateNotFound, id));
        }

        repository.Delete(entryTemplate);

        await unitOfWork.SaveChangesAsync();
    }
}
