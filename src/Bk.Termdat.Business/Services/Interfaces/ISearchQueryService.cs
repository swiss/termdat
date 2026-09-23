using System.Collections.Generic;
using System.Threading.Tasks;
using Bk.Termdat.Business.Resource.Models;

namespace Bk.Termdat.Business.Services.Interfaces;

public interface ISearchQueryService
{
    Task<SearchPage> Search(SearchQuery query);
    Task<IEnumerable<string>> Recommend(string searchPhrase, IEnumerable<int> languageIds = null);
}
