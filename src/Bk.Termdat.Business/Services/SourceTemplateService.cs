using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Bk.Termdat.Business.Domain;
using Bk.Termdat.Business.Domain.Models;
using Bk.Termdat.Business.Repositories;
using Bk.Termdat.Business.Resource.Models;
using Bk.Termdat.Business.Services.Interfaces;
using Bk.Termdat.Common.Resources;
using Bk.Termdat.CrossCutting;

namespace Bk.Termdat.Business.Services;

public class SourceTemplateService : ISourceTemplateService
{
    private readonly ISourceTemplateRepository _sourceTemplateRepository;
    private readonly IUserAccountRepository _userAccountRepository;
    private readonly IIdentityService _identityService;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;

    public SourceTemplateService(ISourceTemplateRepository sourceTemplateRepository, IIdentityService identityService, IUserAccountRepository userAccountRepository, IMapper mapper, IUnitOfWork unitOfWork)
    {
        _sourceTemplateRepository = sourceTemplateRepository;
        _userAccountRepository = userAccountRepository;
        _identityService = identityService;
        _mapper = mapper;
        _unitOfWork = unitOfWork;
    }

    public SourceTemplateDefaults GetAllDefaults()
    {
        return new SourceTemplateDefaults
        {
            De = SourceTemplateDefaultTexts(CultureInfo.GetCultureInfo(nameof(IsoCode.De))),
            Fr = SourceTemplateDefaultTexts(CultureInfo.GetCultureInfo(nameof(IsoCode.Fr))),
            It = SourceTemplateDefaultTexts(CultureInfo.GetCultureInfo(nameof(IsoCode.It))),
            Rm = SourceTemplateDefaultTexts(CultureInfo.GetCultureInfo(nameof(IsoCode.Rm))),
            En = SourceTemplateDefaultTexts(CultureInfo.GetCultureInfo(nameof(IsoCode.En)))
        };
    }

    private static SourceTemplateDefault SourceTemplateDefaultTexts(CultureInfo cultureInfo)
    {
        return new SourceTemplateDefault
        {
            FederalLawSr = BackendTexts.ResourceManager.GetString(nameof(BackendTexts.SourceTemplateFederalLawSr), cultureInfo),
            FederalLawAs = BackendTexts.ResourceManager.GetString(nameof(BackendTexts.SourceTemplateFederalLawAs), cultureInfo),
            FederalLawBbl = BackendTexts.ResourceManager.GetString(nameof(BackendTexts.SourceTemplateFederalLawBbl), cultureInfo),
            FederalCourt = BackendTexts.ResourceManager.GetString(nameof(BackendTexts.SourceTemplateFederalCourt), cultureInfo),
            FederalCouncilMessage = BackendTexts.ResourceManager.GetString(nameof(BackendTexts.SourceTemplateBundesratMessage), cultureInfo),
            Internet = BackendTexts.ResourceManager.GetString(nameof(BackendTexts.SourceTemplateInternet), cultureInfo)
        };
    }

    public async Task<IEnumerable<SourceTemplateSummary>> GetAllForCurrentUser()
    {
        var user = await _userAccountRepository.GetByEmail(_identityService.GetCurrentEmail());

        var sourceTemplates = await _sourceTemplateRepository.GetAll(user.Id);

        return _mapper.Map<IEnumerable<SourceTemplateSummary>>(sourceTemplates);
    }

    public async Task<IEnumerable<string>> GetAllForCurrentUserAsText(string isoCode)
    {
        ArgumentNullException.ThrowIfNull(isoCode);

        var user = await _userAccountRepository.GetByEmail(_identityService.GetCurrentEmail());

        var sourceTemplates = await _sourceTemplateRepository.GetAll(user.Id);

        var listOfSourceTemplatesAsString = isoCode.ToUpperInvariant() switch
        {
            "DE" => sourceTemplates.Where(y => !string.IsNullOrEmpty(y.TextDe)).Select(y => y.TextDe).ToList(),
            "FR" => sourceTemplates.Where(y => !string.IsNullOrEmpty(y.TextFr)).Select(y => y.TextFr).ToList(),
            "IT" => sourceTemplates.Where(y => !string.IsNullOrEmpty(y.TextIt)).Select(y => y.TextIt).ToList(),
            "EN" => sourceTemplates.Where(y => !string.IsNullOrEmpty(y.TextEn)).Select(y => y.TextEn).ToList(),
            "RM" => sourceTemplates.Where(y => !string.IsNullOrEmpty(y.TextRm)).Select(y => y.TextRm).ToList(),
            _ => sourceTemplates.Where(y => !string.IsNullOrEmpty(y.TextDe)).Select(y => y.TextDe).ToList(),
        };

        return listOfSourceTemplatesAsString;
    }

    public async Task Create(SourceTemplateModification sourceTemplateCreate)
    {
        var currentEmail = _identityService.GetCurrentEmail();

        var user = await _userAccountRepository.GetByEmail(currentEmail);

        var newSourceTemplate = _mapper.Map<SourceTemplate>(sourceTemplateCreate);

        newSourceTemplate.UserId = user.Id;
        newSourceTemplate.Created = DateTime.UtcNow;
        newSourceTemplate.CreatedBy = currentEmail;
        newSourceTemplate.Modified = DateTime.UtcNow;
        newSourceTemplate.ModifiedBy = currentEmail;

        await _sourceTemplateRepository.Create(newSourceTemplate);

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task Update(int id, SourceTemplateModification sourceTemplateUpdate)
    {
        var currentEmail = _identityService.GetCurrentEmail();

        var user = await _userAccountRepository.GetByEmail(currentEmail);

        var oldSourceTemplate = await _sourceTemplateRepository.GetById(id);

        var updatedSourceTemplate = _mapper.Map<SourceTemplate>(sourceTemplateUpdate);

        updatedSourceTemplate.Id = id;
        updatedSourceTemplate.UserId = user.Id;
        updatedSourceTemplate.Created = oldSourceTemplate.Created;
        updatedSourceTemplate.CreatedBy = oldSourceTemplate.CreatedBy;
        updatedSourceTemplate.Modified = DateTime.UtcNow;
        updatedSourceTemplate.ModifiedBy = currentEmail;

        _sourceTemplateRepository.Update(oldSourceTemplate, updatedSourceTemplate);

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task Delete(int id)
    {
        var oldTemplate = await _sourceTemplateRepository.GetById(id);

        _sourceTemplateRepository.Delete(oldTemplate);

        await _unitOfWork.SaveChangesAsync();
    }
}
