using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Bk.Termdat.Business.Public.Api.Mapping;
using Bk.Termdat.Business.Public.Api.Resource.Models;
using Bk.Termdat.Business.Repositories;

namespace Bk.Termdat.Business.Public.Api.Services;

public class CollectionService : ICollectionService
{
    private readonly ICollectionRepository collectionRepository;
    private readonly ILanguageValidationService languageValidationService;
    private readonly IMapper mapper;

    public CollectionService(ICollectionRepository collectionRepository, ILanguageValidationService languageValidationService, IMapper mapper)
    {
        this.collectionRepository = collectionRepository;
        this.languageValidationService = languageValidationService;
        this.mapper = mapper;
    }

    public async Task<IEnumerable<CollectionSummary>> GetAllActiveApi(string languageIsoCode)
    {
        var inLanguageId = await languageValidationService.ValidateLanguageIsoCode(languageIsoCode);

        var collections = await collectionRepository.GetAllActiveApi();
        return mapper.Map<IEnumerable<CollectionSummary>>(collections, options => { options.Items.Add(PublicApiMappingProfile.LanguageId, inLanguageId); });
    }
}
