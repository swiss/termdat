using System.Collections.Generic;
using System.Threading.Tasks;
using Bk.Termdat.Business.Domain.Models;
using Bk.Termdat.Business.Domain.Projections;

namespace Bk.Termdat.Business.Repositories;

public interface IClassificationRepository
{
    Task<IEnumerable<Classification>> GetAll();
    Task<IEnumerable<ClassificationWithEntryCount>> GetAllDetails();
    Task<Classification> GetById(int id);
}
