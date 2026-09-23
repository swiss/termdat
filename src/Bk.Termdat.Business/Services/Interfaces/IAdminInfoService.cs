using System.Collections.Generic;
using System.Threading.Tasks;
using Bk.Termdat.Business.Resource.Models;

namespace Bk.Termdat.Business.Services.Interfaces;

public interface IAdminInfoService
{
    Task<IEnumerable<AdminInfoSummary>> GetAll();
    Task<AdminInfoDetail> GetByCode(string code);
    Task<AdminInfoDetail> GetOrCreateAdminInfos(AdminInfoSummary summary);
    Task<AdminInfoUpdate> GetDetails(int id);
    Task<IEnumerable<AdminInfoDetail>> GetAllDetails();
    Task<AdminInfoDetail> Create(AdminInfoCreate adminInfoCreate);
    Task<AdminInfoDetail> Update(AdminInfoUpdate adminInfoUpdate);
    Task Delete(int id);
}
