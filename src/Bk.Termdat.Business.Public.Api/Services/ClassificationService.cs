using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Bk.Termdat.Business.Public.Api.Mapping;
using Bk.Termdat.Business.Public.Api.Resource.Models;
using Bk.Termdat.Business.Repositories;

namespace Bk.Termdat.Business.Public.Api.Services;

public class ClassificationService : IClassificationService
{
    private readonly IClassificationRepository classificationRepository;
    private readonly ILanguageValidationService languageValidationService;
    private readonly IMapper mapper;

    public ClassificationService(IClassificationRepository classificationRepository, ILanguageValidationService languageValidationService, IMapper mapper)
    {
        this.classificationRepository = classificationRepository;
        this.languageValidationService = languageValidationService;
        this.mapper = mapper;
    }

    public async Task<IEnumerable<ClassificationSummary>> GetAll(string languageIsoCode)
    {
        var inLanguageId = await languageValidationService.ValidateLanguageIsoCode(languageIsoCode);

        var classifications = await classificationRepository.GetAll();

        return mapper.Map<IEnumerable<ClassificationSummary>>(classifications, options => { options.Items.Add(PublicApiMappingProfile.LanguageId, inLanguageId); });
    }
}
