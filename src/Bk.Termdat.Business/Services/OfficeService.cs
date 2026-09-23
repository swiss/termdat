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

namespace Bk.Termdat.Business.Services;

public class OfficeService : IOfficeService
{
    private readonly IMapper mapper;
    private readonly IOfficeRepository officeRepository;
    private readonly IUnitOfWork unitOfWork;
    private readonly IIdentityService identityService;
    private readonly ILanguageDescriptionMapper languageDescriptionMapper;

    public OfficeService(
        IMapper mapper,
        IOfficeRepository officeRepository,
        IIdentityService identityService,
        IUnitOfWork unitOfWork,
        ILanguageDescriptionMapper languageDescriptionMapper)
    {
        this.mapper = mapper;
        this.officeRepository = officeRepository;
        this.identityService = identityService;
        this.unitOfWork = unitOfWork;
        this.languageDescriptionMapper = languageDescriptionMapper;
    }

    public async Task<OfficeDetail> Create(OfficeCreate officeCreate)
    {
        var mappedOffice = mapper.Map<Office>(officeCreate);
        mappedOffice.Created = System.DateTime.UtcNow;
        mappedOffice.CreatedBy = identityService.GetCurrentEmail();
        mappedOffice.Modified = System.DateTime.UtcNow;
        mappedOffice.ModifiedBy = identityService.GetCurrentEmail();

        mappedOffice.Translations = (await languageDescriptionMapper.Map<OfficeTranslation>(officeCreate)).ToList();

        await officeRepository.Create(mappedOffice);

        await unitOfWork.SaveChangesAsync();

        var newOffice = await officeRepository.GetById(mappedOffice.Id);
        return mapper.Map<OfficeDetail>(newOffice);
    }

    public async Task<IEnumerable<OfficeSummary>> GetAll()
    {
        var offices = await officeRepository.GetAll();
        return mapper.Map<IEnumerable<OfficeSummary>>(offices);
    }

    public async Task<IEnumerable<OfficeDetail>> GetAllDetails()
    {
        var offices = await officeRepository.GetAllDetails();
        return mapper.Map<IEnumerable<OfficeDetail>>(offices);
    }

    public async Task Delete(int id)
    {
        var office = await officeRepository.GetById(id);

        if (office == null)
        {
            throw new EntityNotFoundException(string.Format(BackendTexts.OfficeNotFound, id));
        }

        var entryCount = await officeRepository.GetEntryCountById(id);
        if (entryCount > default(int))
        {
            throw new BusinessValidationException(nameof(Office), BackendTexts.EntryOfficeDeleteFailedHasEntries);
        }

        officeRepository.Delete(office);

        await unitOfWork.SaveChangesAsync();
    }

    public async Task<OfficeUpdate> GetDetails(int id)
    {
        var office = await officeRepository.GetById(id);
        if (office is null)
        {
            throw new EntityNotFoundException(string.Format(BackendTexts.OfficeNotFound, id));
        }

        return mapper.Map<OfficeUpdate>(office);
    }

    public async Task<OfficeDetail> Update(OfficeUpdate officeUpdate)
    {
        var databaseOffice = await officeRepository.GetById(officeUpdate.Id);

        if (databaseOffice == null)
        {
            throw new EntityNotFoundException(string.Format(BackendTexts.OfficeNotFound, officeUpdate.Id));
        }

        var mappedOffice = mapper.Map<Office>(officeUpdate);
        mappedOffice.Modified = System.DateTime.UtcNow;
        mappedOffice.ModifiedBy = identityService.GetCurrentEmail();
        mappedOffice.Created = databaseOffice.Created;
        mappedOffice.CreatedBy = databaseOffice.CreatedBy;
        mappedOffice.Translations = (await languageDescriptionMapper.Map<OfficeTranslation>(officeUpdate)).ToList();

        officeRepository.Update(databaseOffice, mappedOffice);

        await unitOfWork.SaveChangesAsync();

        var office = await officeRepository.GetById(databaseOffice.Id);
        var officeDetail = mapper.Map<OfficeDetail>(office);
        return officeDetail;
    }
}
