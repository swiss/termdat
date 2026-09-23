using System.Collections.Generic;
using System.Threading.Tasks;
using Bk.Termdat.Business.Domain.Models;
using Bk.Termdat.Business.Domain.Projections;

namespace Bk.Termdat.Business.Repositories;

public interface IAdminInfoRepository
{
    Task<IEnumerable<AdminInfo>> GetAll();
    Task<AdminInfo> GetById(int id);
    Task<AdminInfo> GetByCode(string code);
    Task<IEnumerable<AdminInfo>> GetByCodes(IEnumerable<string> codes);
    Task<IEnumerable<AdminInfoWithEntryCount>> GetAllDetails();
    Task Create(AdminInfo adminInfo);
    void Update(AdminInfo existing, AdminInfo update);
    void Delete(AdminInfo adminInfo);
}
