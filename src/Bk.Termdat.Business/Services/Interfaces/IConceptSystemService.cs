using System.Collections.Generic;
using System.Threading.Tasks;
using Bk.Termdat.Business.Resource.Models;

namespace Bk.Termdat.Business.Services.Interfaces;

public interface IConceptSystemService
{
    Task<IEnumerable<ConceptSystemSummary>> GetAll();
    Task<IEnumerable<ConceptSystemDetail>> GetAllDetails();
    Task<ConceptSystemUpdate> GetDetail(int id);
    Task<ConceptSystemDetail> Create(ConceptSystemCreate conceptSystemCreate);
    Task<ConceptSystemDetail> Update(ConceptSystemUpdate conceptSystemUpdate);
    Task Delete(int id);
}
