using System;
using System.Threading.Tasks;

namespace Bk.Termdat.Business.Domain;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync();

    int SaveChanges();

    Task ExecuteInTransactionAsync(Func<Task> action);
}
