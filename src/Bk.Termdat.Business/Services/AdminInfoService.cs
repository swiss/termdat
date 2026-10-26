using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using AutoMapper;
using Bk.Termdat.Business.Clients;
using Bk.Termdat.Business.Domain;
using Bk.Termdat.Business.Domain.Models;
using Bk.Termdat.Business.Repositories;
using Bk.Termdat.Business.Resource.Models;
using Bk.Termdat.Business.Services.Interfaces;
using Bk.Termdat.Common.Resources;
using Bk.Termdat.CrossCutting.Exceptions;
using Microsoft.Extensions.Logging;

namespace Bk.Termdat.Business.Services;

public class AdminInfoService : IAdminInfoService
{
    private readonly IMapper mapper;
    private readonly IAdminInfoRepository adminInfoRepository;
    private readonly IEntryRepository entryRepository;
    private readonly ISearchQueueClient searchQueueClient;
    private readonly IUnitOfWork unitOfWork;
    private readonly IIdentityService identityService;
    private readonly ILogger<AdminInfoService> logger;

    public AdminInfoService(IMapper mapper, IAdminInfoRepository adminInfoRepository, IEntryRepository entryRepository, ISearchQueueClient searchQueueClient, IIdentityService identityService, IUnitOfWork unitOfWork, ILogger<AdminInfoService> logger)
    {
        this.mapper = mapper;
        this.adminInfoRepository = adminInfoRepository;
        this.entryRepository = entryRepository;
        this.searchQueueClient = searchQueueClient;
        this.unitOfWork = unitOfWork;
        this.identityService = identityService;
        this.logger = logger;
    }

    public async Task<IEnumerable<AdminInfoSummary>> GetAll()
    {
        var adminInfos = await adminInfoRepository.GetAll();
        return mapper.Map<IEnumerable<AdminInfoSummary>>(adminInfos);
    }

    public async Task<AdminInfoDetail> GetByCode(string code)
    {
        var adminInfo = await adminInfoRepository.GetByCode(code);
        return mapper.Map<AdminInfoDetail>(adminInfo);
    }

    public async Task<IEnumerable<AdminInfoDetail>> GetAllDetails()
    {
        var adminInfos = await adminInfoRepository.GetAllDetails();
        return mapper.Map<IEnumerable<AdminInfoDetail>>(adminInfos);
    }

    public async Task<AdminInfoUpdate> GetDetails(int id)
    {
        var adminInfo = await adminInfoRepository.GetById(id);
        if (adminInfo is null)
        {
            throw new EntityNotFoundException(string.Format(CultureInfo.InvariantCulture, BackendTexts.AdminInfoNotFound, id));
        }

        return mapper.Map<AdminInfoUpdate>(adminInfo);
    }

    public async Task<AdminInfoDetail> GetOrCreateAdminInfos(AdminInfoSummary summary)
    {
        ArgumentNullException.ThrowIfNull(summary);

        var existing = await GetByCode(summary.Code);

        if (existing == null)
        {
            return await Create(mapper.Map<AdminInfoCreate>(summary));
        }
        else
        {
            return existing;
        }
    }

    public async Task<AdminInfoDetail> Create(AdminInfoCreate adminInfoCreate)
    {
        var mappedAdminInfo = mapper.Map<AdminInfo>(adminInfoCreate);
        mappedAdminInfo.Created = DateTime.UtcNow;
        mappedAdminInfo.CreatedBy = identityService.GetCurrentEmail();
        mappedAdminInfo.Modified = DateTime.UtcNow;
        mappedAdminInfo.ModifiedBy = identityService.GetCurrentEmail();

        await adminInfoRepository.Create(mappedAdminInfo);

        await unitOfWork.SaveChangesAsync();

        var newAdminInfo = await adminInfoRepository.GetById(mappedAdminInfo.Id);
        return mapper.Map<AdminInfoDetail>(newAdminInfo);
    }

    public async Task<AdminInfoDetail> Update(AdminInfoUpdate adminInfoUpdate)
    {
        ArgumentNullException.ThrowIfNull(adminInfoUpdate);

        var databaseAdminInfo = await adminInfoRepository.GetById(adminInfoUpdate.Id);

        if (databaseAdminInfo == null)
        {
            throw new EntityNotFoundException(string.Format(CultureInfo.InvariantCulture, BackendTexts.AdminInfoNotFound, adminInfoUpdate.Id));
        }

        var mappedAdminInfo = mapper.Map<AdminInfo>(adminInfoUpdate);
        mappedAdminInfo.Modified = DateTime.UtcNow;
        mappedAdminInfo.ModifiedBy = identityService.GetCurrentEmail();
        mappedAdminInfo.Created = databaseAdminInfo.Created;
        mappedAdminInfo.CreatedBy = databaseAdminInfo.CreatedBy;

        adminInfoRepository.Update(databaseAdminInfo, mappedAdminInfo);

        await unitOfWork.SaveChangesAsync();

        var adminInfo = await adminInfoRepository.GetById(databaseAdminInfo.Id);
        var adminInfoDetail = mapper.Map<AdminInfoDetail>(adminInfo);
        return adminInfoDetail;
    }

    public async Task Delete(int id)
    {
        var adminInfo = await adminInfoRepository.GetById(id);

        if (adminInfo == null)
        {
            throw new EntityNotFoundException(string.Format(CultureInfo.InvariantCulture, BackendTexts.AdminInfoNotFound, id));
        }

        var entryIds = await entryRepository.GetIdsByAdminInfoId(adminInfo.Id);

        adminInfoRepository.Delete(adminInfo);

        await unitOfWork.SaveChangesAsync();

        try
        {
            await searchQueueClient.QueueIndexUpdateByIds(entryIds);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Error while updating entries in search index after admininfo '{Id}' was deleted", adminInfo.Id);
        }
    }
}
