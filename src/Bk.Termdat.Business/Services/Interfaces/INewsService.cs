using System.Collections.Generic;
using System.Threading.Tasks;
using Bk.Termdat.Business.Resource.Models;

namespace Bk.Termdat.Business.Services.Interfaces;

public interface INewsService
{
    Task<IEnumerable<NewsItemSummary>> GetAll();
    Task<IEnumerable<NewsItemSummary>> GetAllActive();
    Task<NewsItemUpdate> GetDetails(int id);
    Task<IEnumerable<NewsItemDetail>> GetAllDetails();
    Task<NewsItemDetail> Update(NewsItemUpdate newsItemUpdate);
    Task<NewsItemDetail> Create(NewsItemCreate newsItemCreate);
    Task Delete(int id);
    Task MoveItemUp(int id);
    Task MoveItemDown(int id);
}
