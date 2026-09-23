using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
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

public class SubjectService : ISubjectService
{
    private readonly ISubjectRepository subjectRepository;
    private readonly IDomainRepository domainRepository;
    private readonly IDomainService domainService;
    private readonly IUnitOfWork unitOfWork;
    private readonly IMapper mapper;
    private readonly ILogger<SubjectService> logger;
    private readonly IIdentityService identityService;
    private readonly ILanguageDescriptionMapper languageDescriptionMapper;

    public SubjectService(
        ISubjectRepository subjectRepository,
        IDomainRepository domainRepository,
        IDomainService domainService,
        IIdentityService identityService,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILanguageDescriptionMapper languageDescriptionMapper,
        ILogger<SubjectService> logger)
    {
        this.subjectRepository = subjectRepository;
        this.domainRepository = domainRepository;
        this.domainService = domainService;
        this.identityService = identityService;
        this.unitOfWork = unitOfWork;
        this.mapper = mapper;
        this.languageDescriptionMapper = languageDescriptionMapper;
        this.logger = logger;
    }

    public async Task<IEnumerable<SubjectSummary>> GetAll()
    {
        var subjects = await subjectRepository.GetAll();
        return mapper.Map<IEnumerable<SubjectSummary>>(subjects);
    }

    public async Task<IEnumerable<SubjectDetail>> GetAllDetails()
    {
        var subjects = await subjectRepository.GetAllDetails();
        var subjectDetails = mapper.Map<IEnumerable<SubjectDetail>>(subjects).ToList();

        var domains = (await domainRepository.GetAllDetails()).ToList();

        subjectDetails.ForEach(subjectDetail =>
        {
            subjectDetail.Domains = mapper.Map<IEnumerable<DomainDetail>>(domains.Where(domain => domain.Domain.SubjectId == subjectDetail.Id))
                .OrderBy(domain => domain.Name)
                .ToList();
        });

        return subjectDetails;
    }

    public async Task<SubjectDetail> Create(SubjectModification subjectCreate)
    {
        var newSubject = new Subject
        {
            Created = DateTime.UtcNow,
            CreatedBy = identityService.GetCurrentEmail(),
            Modified = DateTime.UtcNow,
            ModifiedBy = identityService.GetCurrentEmail(),
            Translations = (await languageDescriptionMapper.Map<SubjectTranslation>(subjectCreate)).ToList()
        };

        await unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            await subjectRepository.Create(newSubject);
            await unitOfWork.SaveChangesAsync();

            foreach (var domain in subjectCreate.Domains)
            {
                domain.SubjectId = newSubject.Id;
                await domainService.Create(domain);
            }
        });

        logger.LogDebug("Created new Subject with Id {id}", newSubject.Id);

        var subject = await subjectRepository.GetById(newSubject.Id);
        return mapper.Map<SubjectDetail>(subject);
    }

    public async Task<SubjectModification> GetDetail(int id)
    {
        var subject = await subjectRepository.GetById(id)
            ?? throw new EntityNotFoundException(string.Format(BackendTexts.SubjectNotFound, id));

        return mapper.Map<SubjectModification>(subject);
    }

    public async Task<SubjectDetail> Update(SubjectModification subjectUpdate)
    {
        var subjectId = subjectUpdate?.Id
            ?? throw new ArgumentException("Cannot update subject with no id.");
        var databaseSubject = (await subjectRepository.GetById(subjectId))
            ?? throw new EntityNotFoundException(string.Format(BackendTexts.SubjectNotFound, subjectUpdate.Id));

        var mappedSubject = mapper.Map<Subject>(subjectUpdate);
        mappedSubject.Created = databaseSubject.Created;
        mappedSubject.CreatedBy = databaseSubject.CreatedBy;
        mappedSubject.Modified = DateTime.UtcNow;
        mappedSubject.ModifiedBy = identityService.GetCurrentEmail();
        mappedSubject.Translations = (await languageDescriptionMapper.Map<SubjectTranslation>(subjectUpdate)).ToList();

        subjectRepository.Update(databaseSubject, mappedSubject);

        foreach (var domain in subjectUpdate.Domains)
        {
            domain.SubjectId = databaseSubject.Id;
            if (domain.Id.HasValue)
            {
                // This should not happen as long as the user can only update one domain at once
                await domainService.Update(domain);
            }
            else
            {
                await domainService.Create(domain);
            }
        }

        await unitOfWork.SaveChangesAsync();

        var subject = await subjectRepository.GetById(databaseSubject.Id);
        return mapper.Map<SubjectDetail>(subject);
    }

    public async Task Delete(int id)
    {
        var subject = await subjectRepository.GetById(id);
        if (subject is null)
        {
            throw new EntityNotFoundException(string.Format(BackendTexts.SubjectNotFound, id));
        }

        var entryCount = await subjectRepository.GetEntryCountById(id);
        if (entryCount > 0)
        {
            throw new BusinessValidationException(nameof(Subject), BackendTexts.SubjectDeleteFailedHasEntries);
        }

        subjectRepository.Delete(subject);

        await unitOfWork.SaveChangesAsync();
    }
}
