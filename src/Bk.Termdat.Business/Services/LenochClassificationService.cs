using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Bk.Termdat.Business.Repositories;
using Bk.Termdat.Business.Services.Interfaces;
using ResourceModels = Bk.Termdat.Business.Resource.Models;

namespace Bk.Termdat.Business.Services;

public class LenochClassificationService : ILenochClassificationService
{
    private readonly IMapper mapper;
    private readonly ILenochClassificationRepository lenochClassificationRepository;
    private readonly ILenochClassificationSubjectDomainMappingRepository lenochClassificationSubjectDomainMappingRepository;

    public LenochClassificationService(ILenochClassificationRepository lenochClassificationRepository,
        ILenochClassificationSubjectDomainMappingRepository lenochClassificationSubjectDomainMappingRepository,
        IMapper mapper)
    {
        this.lenochClassificationRepository = lenochClassificationRepository;
        this.lenochClassificationSubjectDomainMappingRepository = lenochClassificationSubjectDomainMappingRepository;
        this.mapper = mapper;
    }

    public async Task<IEnumerable<ResourceModels.LenochClassificationSummary>> GetAll()
    {
        var lenochClassifications = await lenochClassificationRepository.GetAll();
        return mapper.Map<IEnumerable<ResourceModels.LenochClassificationSummary>>(lenochClassifications);
    }

    public async Task<IEnumerable<ResourceModels.LenochClassificationDetail>> GetAllDetails()
    {
        var mappings = (await lenochClassificationSubjectDomainMappingRepository.GetAll()).ToList();
        var lenochClassifications = (await lenochClassificationRepository.GetAllDetails()).ToList();

        var result = mappings.Select(mapping =>
        {
            var lenochClassification = lenochClassifications.Single(lc => lc.Entity.Id == mapping.LenochClassificationId);
            var mappedMapping = mapper.Map<ResourceModels.LenochClassificationDetail>(mapping);
            var mappedLenochClassification = mapper.Map<ResourceModels.LenochClassificationDetail>(lenochClassification);

            mappedLenochClassification.Domain = mappedMapping.Domain;
            mappedLenochClassification.Subject = mappedMapping.Subject;

            return mappedLenochClassification;
        }).ToList();

        var mappedClassificationIds = mappings.Select(m => m.LenochClassificationId)
            .Distinct()
            .ToList();
        var lenochClassificationsWithoutMapping = lenochClassifications.Where(lc => !mappedClassificationIds.Contains(lc.Entity.Id))
            .Select(lc => mapper.Map<ResourceModels.LenochClassificationDetail>(lc))
            .ToList();

        result.AddRange(lenochClassificationsWithoutMapping);

        return result.OrderBy(r => r.Code);
    }
}
