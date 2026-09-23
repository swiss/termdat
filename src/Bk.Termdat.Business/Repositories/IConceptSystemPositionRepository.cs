using System.Collections.Generic;
using System.Threading.Tasks;
using Bk.Termdat.Business.Domain.Models;

namespace Bk.Termdat.Business.Repositories;

public interface IConceptSystemPositionRepository
{
    Task<ConceptSystemPosition> GetById(int id);
    Task<IEnumerable<ConceptSystemPosition>> GetByCollectionId(int collectionId);
}
