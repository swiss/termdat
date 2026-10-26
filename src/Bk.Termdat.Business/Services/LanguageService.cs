using System;
using System.Collections.Generic;
using System.Globalization;
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

public class LanguageService : ILanguageService
{
    private readonly IMapper mapper;
    private readonly ILanguageRepository languageRepository;
    private readonly IIdentityService identityService;
    private readonly IUnitOfWork unitOfWork;
    private readonly ILanguageTranslationMapper languageTranslationMapper;

    public LanguageService(IMapper mapper, ILanguageRepository languageRepository, IIdentityService identityService, IUnitOfWork unitOfWork, ILanguageTranslationMapper languageTranslationMapper)
    {
        this.mapper = mapper;
        this.languageRepository = languageRepository;
        this.identityService = identityService;
        this.unitOfWork = unitOfWork;
        this.languageTranslationMapper = languageTranslationMapper;
    }

    public async Task<LanguageDetail> GetById(int id)
    {
        var language = await languageRepository.GetById(id);
        return mapper.Map<LanguageDetail>(language);
    }

    public async Task<IEnumerable<LanguageSummary>> GetTranslationByIsoCode(string isoCode)
    {
        var languages = await languageRepository.GetTranslationByIsoCode(isoCode);
        return mapper.Map<IEnumerable<LanguageSummary>>(languages);
    }

    public async Task<IEnumerable<LanguageDetail>> GetAllDetails()
    {
        var languages = await languageRepository.GetAllDetails();
        return mapper.Map<IEnumerable<LanguageDetail>>(languages);
    }

    public async Task<LanguageUpdate> GetDetail(int id)
    {
        var language = await languageRepository.GetById(id);
        if (language == null)
        {
            throw new EntityNotFoundException(string.Format(CultureInfo.InvariantCulture, BackendTexts.LanguageNotFound, id));
        }
        var mappedLanguage = mapper.Map<LanguageUpdate>(language);

        return mappedLanguage;
    }

    public async Task<LanguageDetail> Create(LanguageCreate languageCreate)
    {
        var mappedLanguage = mapper.Map<Language>(languageCreate);
        mappedLanguage.Modified = DateTime.UtcNow;
        mappedLanguage.ModifiedBy = identityService.GetCurrentEmail();
        mappedLanguage.Created = DateTime.UtcNow;
        mappedLanguage.CreatedBy = identityService.GetCurrentEmail();

        mappedLanguage.Translations = await MapLanguageTranslations(languageCreate, 0);

        await languageRepository.Create(mappedLanguage);

        await unitOfWork.SaveChangesAsync();

        var language = await languageRepository.GetById(mappedLanguage.Id);
        return mapper.Map<LanguageDetail>(language);
    }

    public async Task<LanguageDetail> Update(LanguageUpdate languageUpdate)
    {
        ArgumentNullException.ThrowIfNull(languageUpdate);

        var databaseLanguage = await languageRepository.GetById(languageUpdate.Id);

        if (databaseLanguage == null)
        {
            throw new EntityNotFoundException(BackendTexts.LanguageNotFound);
        }

        if (databaseLanguage.IsSystemLanguage && languageUpdate.IsoCode != databaseLanguage.IsoCode)
        {
            throw new CanNotBeChangedException(BackendTexts.SystemLanguagesIsoCodeCanNotBeChanged);
        }

        var mappedLanguage = mapper.Map<Language>(languageUpdate);
        mappedLanguage.Modified = DateTime.UtcNow;
        mappedLanguage.ModifiedBy = identityService.GetCurrentEmail();
        mappedLanguage.Created = databaseLanguage.Created;
        mappedLanguage.CreatedBy = databaseLanguage.CreatedBy;

        mappedLanguage.Translations = await MapLanguageTranslations(languageUpdate, languageUpdate.Id);

        languageRepository.Update(databaseLanguage, mappedLanguage);

        await unitOfWork.SaveChangesAsync();

        var language = await languageRepository.GetById(databaseLanguage.Id);
        return mapper.Map<LanguageDetail>(language);
    }

    public int GetNextSort()
    {
        return languageRepository.GetNextSort();
    }

    private async Task<ICollection<LanguageTranslation>> MapLanguageTranslations(ILanguageModification languageModification, int languageId)
    {
        var languages = (await languageRepository.GetAll()).ToList();

        return languageTranslationMapper.Map(languageModification, languages, languageId).ToList();
    }
}
