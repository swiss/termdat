using System.Collections.Generic;
using System.Threading.Tasks;
using Bk.Termdat.Business.Domain.Models;

namespace Bk.Termdat.Business.Repositories;

public interface INewsRepository
{
    Task<NewsItem> GetById(int id);
    Task<IEnumerable<NewsItem>> GetAll();
    Task<IEnumerable<NewsItem>> GetAllActive();
    Task Update(NewsItem existing, NewsItem update);
    Task Create(NewsItem newsItem);
    void Delete(NewsItem newsItem);
    Task<int> GetNextSortIndex();
    Task<NewsItem?> GetNextBySortIndex(int sortIndex);
    Task<NewsItem?> GetPreviousBySortIndex(int sortIndex);
}
