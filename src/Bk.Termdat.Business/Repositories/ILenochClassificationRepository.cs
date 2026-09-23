using System.Collections.Generic;
using System.Threading.Tasks;
using Bk.Termdat.Business.Domain.Models;
using Bk.Termdat.Business.Domain.Projections;

namespace Bk.Termdat.Business.Repositories;

public interface ILenochClassificationRepository
{
    Task<IEnumerable<LenochClassification>> GetAll();
    Task<IEnumerable<LenochClassification>> GetByIds(IEnumerable<int> lenochClassificationIds);
    Task<IEnumerable<EntityWithEntryCount<LenochClassification>>> GetAllDetails();
}
