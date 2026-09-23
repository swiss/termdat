using System.Collections.Generic;
using System.Threading.Tasks;
using Bk.Termdat.Business.Domain.Models;
using Bk.Termdat.Business.Domain.Projections;

namespace Bk.Termdat.Business.Repositories;

public interface ICollectionRepository
{
    Task<IEnumerable<Collection>> GetAll();
    Task<IEnumerable<Collection>> GetAllActive();
    Task<IEnumerable<Collection>> GetAuthorizedByIds(IEnumerable<int> authorizedCollectionIds);
    Task<IEnumerable<Collection>> GetAllActiveApi();
    Task<IEnumerable<CollectionWithEntryCount>> GetAllDetails();
    Task<IEnumerable<Collection>> GetAccessibleActiveAndInactive(IEnumerable<int> allowedInactiveIds);
    Task<IEnumerable<Collection>> GetByIds(IEnumerable<int> collectionIds);
    Task<Collection> GetById(int id);
    Task Create(Collection collection);
    void Update(Collection existing, Collection update);
    Task<CollectionWithEntryCount> GetByIdForDelete(int id);
    void Delete(Collection collection);
    Task UpdateDeletedFlag(Collection collection, bool deleted, string modifiedBy);
    Task<IEnumerable<int>> GetIdsForLindasExport();
    Task<Collection?> GetByIdForLindasExport(int id);
}
