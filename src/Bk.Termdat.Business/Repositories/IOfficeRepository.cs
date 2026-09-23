using System.Collections.Generic;
using System.Threading.Tasks;
using Bk.Termdat.Business.Domain.Models;
using Bk.Termdat.Business.Domain.Projections;

namespace Bk.Termdat.Business.Repositories;

public interface IOfficeRepository
{
    Task<Office> GetById(int id);
    Task<IEnumerable<Office>> GetAll();
    Task<IEnumerable<OfficeWithEntryCount>> GetAllDetails();
    void Update(Office existing, Office update);
    Task Create(Office office);
    void Delete(Office office);
    Task<int> GetEntryCountById(int id);
}
