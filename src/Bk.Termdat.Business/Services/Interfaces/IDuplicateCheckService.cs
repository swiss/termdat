using System.IO;
using System.Threading.Tasks;
using Bk.Termdat.Business.Resource.Models;

namespace Bk.Termdat.Business.Services.Interfaces;

public interface IDuplicateCheckService
{
    Task<DuplicateCheckResponse> GetDuplicateEntryIds(DuplicateCheckRequest request);
    Task<(string fileName, Stream content)> ExportDuplicateCheckXlsx(DuplicateCheckRequest request);
}
