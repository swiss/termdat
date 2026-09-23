using System.Collections.Generic;
using System.Threading.Tasks;
using Bk.Termdat.Business.Domain.Models;
using Bk.Termdat.Business.Domain.Projections;

namespace Bk.Termdat.Business.Repositories;

public interface IConceptSystemRepository
{
    Task<IEnumerable<ConceptSystem>> GetAll();
    Task<IEnumerable<ConceptSystemWithEntryCount>> GetAllDetails();
    Task<ConceptSystem> GetById(int id);
    Task<ConceptSystem> GetByIdForEdit(int id);
    Task Create(ConceptSystem conceptSystem);
    void Update(ConceptSystem existing, ConceptSystem update);
    void Delete(ConceptSystem conceptSystem);
}
