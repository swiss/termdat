using System.Collections.Generic;
using System.Threading.Tasks;
using Bk.Termdat.Business.Domain.Models;

namespace Bk.Termdat.Business.Repositories;

public interface ILenochClassificationSubjectDomainMappingRepository
{
    Task<IEnumerable<LenochClassificationSubjectDomainMapping>> GetAll();
}
