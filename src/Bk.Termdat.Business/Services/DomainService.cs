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
using DomainModels = Bk.Termdat.Business.Domain.Models;

namespace Bk.Termdat.Business.Services;

public class DomainService : IDomainService
{
    private readonly IDomainRepository domainRepository;
    private readonly IUnitOfWork unitOfWork;
    private readonly IMapper mapper;
    private readonly ILanguageDescriptionMapper languageDescriptionMapper;
    private readonly IIdentityService identityService;

    public DomainService(
        IDomainRepository domainRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IIdentityService identityService,
        ILanguageDescriptionMapper languageDescriptionMapper)
    {
        this.domainRepository = domainRepository;
        this.unitOfWork = unitOfWork;
        this.mapper = mapper;
        this.identityService = identityService;
        this.languageDescriptionMapper = languageDescriptionMapper;
    }

    public async Task<IEnumerable<ILanguageDescription>> GetAllDescriptions()
    {
        var domains = await domainRepository.GetAll();
        return mapper.Map<IEnumerable<DomainModification>>(domains);
    }

    public async Task<DomainDetail> Create(DomainModification domainCreate)
    {
        ArgumentNullException.ThrowIfNull(domainCreate);

        var mappedDomain = mapper.Map<DomainModels.Domain>(domainCreate);
        mappedDomain.Created = DateTime.UtcNow;
        mappedDomain.CreatedBy = identityService.GetCurrentEmail();
        mappedDomain.Modified = DateTime.UtcNow;
        mappedDomain.ModifiedBy = identityService.GetCurrentEmail();
        mappedDomain.Translations = (await languageDescriptionMapper.Map<DomainTranslation>(domainCreate)).ToList();

        await domainRepository.Create(mappedDomain);
        await unitOfWork.SaveChangesAsync();

        var domain = await domainRepository.GetById(mappedDomain.Id);
        return mapper.Map<DomainDetail>(domain);
    }

    public async Task<DomainDetail> Update(DomainModification domainUpdate)
    {
        ArgumentNullException.ThrowIfNull(domainUpdate);
        ArgumentNullException.ThrowIfNull(domainUpdate.Id);

        var databaseDomain = await domainRepository.GetById(domainUpdate.Id.Value);

        if (databaseDomain is null)
        {
            throw new EntityNotFoundException(string.Format(BackendTexts.DomainNotFound, domainUpdate.Id));
        }

        var mappedDomain = mapper.Map<DomainModels.Domain>(domainUpdate);
        mappedDomain.SubjectId = databaseDomain.SubjectId;
        mappedDomain.Translations = (await languageDescriptionMapper.Map<DomainTranslation>(domainUpdate)).ToList();
        mappedDomain.Modified = DateTime.UtcNow;
        mappedDomain.ModifiedBy = identityService.GetCurrentEmail();
        mappedDomain.Created = databaseDomain.Created;
        mappedDomain.CreatedBy = databaseDomain.CreatedBy;

        domainRepository.Update(databaseDomain, mappedDomain);

        await unitOfWork.SaveChangesAsync();

        var domain = await domainRepository.GetById(databaseDomain.Id);
        var domainDetail = mapper.Map<DomainDetail>(domain);
        return domainDetail;
    }

    public async Task Delete(int id)
    {
        var domain = await domainRepository.GetById(id);
        if (domain is null)
        {
            throw new EntityNotFoundException(string.Format(BackendTexts.DomainNotFound, id));
        }

        var entryCount = await domainRepository.GetEntryCountById(id);
        if (entryCount > 0)
        {
            throw new BusinessValidationException(nameof(DomainModels.Domain), BackendTexts.DomainDeleteFailedHasEntries);
        }

        domainRepository.Delete(domain);
        await unitOfWork.SaveChangesAsync();
    }
}
