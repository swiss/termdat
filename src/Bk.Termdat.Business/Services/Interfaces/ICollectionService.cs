using System.Collections.Generic;
using System.Threading.Tasks;
using Bk.Termdat.Business.Resource.Models;

namespace Bk.Termdat.Business.Services.Interfaces;

public interface ICollectionService
{
    Task<IEnumerable<CollectionSummary>> GetAll();
    Task<IEnumerable<CollectionSummary>> GetAllActive();
    Task<IEnumerable<CollectionSummary>> GetAllForModification();
    Task<IEnumerable<CollectionDetail>> GetAllDetails();
    Task<IEnumerable<CollectionSummary>> GetAllForCurrentUser();
    Task<CollectionUpdate> GetDetail(int id);
    Task<CollectionDetail> Create(CollectionCreate collectionCreate);
    Task<CollectionDetail> Update(CollectionUpdate collectionUpdate);
    Task<CollectionDetail> GetById(int id);
    Task Delete(int id, bool soft = true);
    Task Restore(int id);
}
