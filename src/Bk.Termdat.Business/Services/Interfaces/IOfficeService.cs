using System.Collections.Generic;
using System.Threading.Tasks;
using Bk.Termdat.Business.Resource.Models;

namespace Bk.Termdat.Business.Services.Interfaces;

public interface IOfficeService
{
    Task<IEnumerable<OfficeSummary>> GetAll();
    Task<OfficeUpdate> GetDetails(int id);
    Task<IEnumerable<OfficeDetail>> GetAllDetails();
    Task<OfficeDetail> Update(OfficeUpdate officeUpdate);
    Task<OfficeDetail> Create(OfficeCreate officeCreate);
    Task Delete(int id);
}
