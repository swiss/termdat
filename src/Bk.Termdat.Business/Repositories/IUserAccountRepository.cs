using System.Collections.Generic;
using System.Threading.Tasks;
using Bk.Termdat.Business.Domain.Models;

namespace Bk.Termdat.Business.Repositories;

public interface IUserAccountRepository
{
    Task<IEnumerable<UserAccount>> GetAll();
    Task<IEnumerable<UserAccount>> GetAllDetailed();
    Task<UserAccount> GetByEmail(string email);
    Task Create(UserAccount userAccount);
    void Update(UserAccount existing, UserAccount update);
    Task<UserAccount> GetDetail(int id);
    Task<UserAccount> GetDetailByEmail(string email);
    Task<UserAccount> GetDetailByEmailForUpdate(string email);
    Task<UserAccount> GetDetailByIdForUpdate(int id);
    void UpdateWithRelations(UserAccount existing, UserAccount update);
}
