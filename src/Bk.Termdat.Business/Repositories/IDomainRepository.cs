using System.Collections.Generic;
using System.Threading.Tasks;
using Bk.Termdat.Business.Domain.Projections;

namespace Bk.Termdat.Business.Repositories;

public interface IDomainRepository
{
    Task<IEnumerable<Domain.Models.Domain>> GetAll();
    Task<IEnumerable<DomainWithEntryCount>> GetAllDetails();
    Task<IEnumerable<Domain.Models.Domain>> GetByIds(IEnumerable<int> domainIds);
    Task<Domain.Models.Domain> GetById(int id);
    Task Create(Domain.Models.Domain domain);
    void Update(Domain.Models.Domain existing, Domain.Models.Domain update);
    void Delete(Domain.Models.Domain domain);
    Task<int> GetEntryCountById(int id);
}
