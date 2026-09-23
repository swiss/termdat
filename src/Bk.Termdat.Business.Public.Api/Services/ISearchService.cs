using System.Collections.Generic;
using System.Threading.Tasks;
using Bk.Termdat.Business.Public.Api.Resource.Models;

namespace Bk.Termdat.Business.Public.Api.Services;

public interface ISearchService
{
    Task<IEnumerable<EntryDetail>> Search(SearchParameter parameter);
}
