using System.Collections.Generic;
using System.Threading.Tasks;
using Bk.Termdat.Business.Public.Api.Resource.Models;

namespace Bk.Termdat.Business.Public.Api.Services;

public interface IEntryService
{
    Task<IEnumerable<EntryDetail>> GetByIds(int[] entryIds, string inLanguageIsoCode, string outLanguageIsoCode);
}
