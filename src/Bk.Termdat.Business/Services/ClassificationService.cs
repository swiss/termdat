using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Bk.Termdat.Business.Repositories;
using Bk.Termdat.Business.Resource.Models;
using Bk.Termdat.Business.Services.Interfaces;

namespace Bk.Termdat.Business.Services;

public class ClassificationService : IClassificationService
{
    private readonly IMapper mapper;
    private readonly IClassificationRepository classificationsRepository;

    public ClassificationService(IMapper mapper, IClassificationRepository classificationsRepository)
    {
        this.mapper = mapper;
        this.classificationsRepository = classificationsRepository;
    }

    public async Task<IEnumerable<ClassificationSummary>> GetAll()
    {
        var classifications = await classificationsRepository.GetAll();
        return mapper.Map<IEnumerable<ClassificationSummary>>(classifications);
    }
}
