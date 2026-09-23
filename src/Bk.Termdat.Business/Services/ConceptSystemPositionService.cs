using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Bk.Termdat.Business.Repositories;
using Bk.Termdat.Business.Resource.Models;
using Bk.Termdat.Business.Services.Interfaces;

namespace Bk.Termdat.Business.Services;

public class ConceptSystemPositionService : IConceptSystemPositionService
{
    private readonly IMapper mapper;
    private readonly IConceptSystemPositionRepository conceptSystemPositionRepository;

    public ConceptSystemPositionService(IMapper mapper, IConceptSystemPositionRepository conceptSystemPositionRepository)
    {
        this.mapper = mapper;
        this.conceptSystemPositionRepository = conceptSystemPositionRepository;
    }

    public async Task<IEnumerable<ConceptSystemPositionSummary>> GetByCollectionId(int collectionId)
    {
        var conceptSystemPositions = await conceptSystemPositionRepository.GetByCollectionId(collectionId);
        return mapper.Map<IEnumerable<ConceptSystemPositionSummary>>(conceptSystemPositions);
    }
}
