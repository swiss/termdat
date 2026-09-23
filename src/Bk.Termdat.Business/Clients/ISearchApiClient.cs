using System.Collections.Generic;
using System.Threading.Tasks;
using Bk.Termdat.Business.Resource.Models;

namespace Bk.Termdat.Business.Clients;

public interface ISearchApiClient
{
    Task<bool> IsAlive();
    Task<SearchResult> Search(SearchFilterParameter searchFilterParameter, SearchFields searchFields);
    Task<IEnumerable<string>> Recommend(string searchPhrase, IEnumerable<int> languageIds = null);
}
