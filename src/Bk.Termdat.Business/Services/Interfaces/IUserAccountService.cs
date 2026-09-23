using System.Collections.Generic;
using System.Threading.Tasks;
using Bk.Termdat.Business.Resource.Models;

namespace Bk.Termdat.Business.Services.Interfaces;

public interface IUserAccountService
{
    Task<IEnumerable<UserAccountSummary>> GetAll();
    Task<UserAccountDetail> CreateOrUpdate();
    Task<IEnumerable<UserAccountDetail>> GetAllDetails();
    Task UpdateLastAction(string email);
    Task<UserAccountDetail> GetDetail(int id);
    Task Update(UserAccountModification userAccountModification);
}
