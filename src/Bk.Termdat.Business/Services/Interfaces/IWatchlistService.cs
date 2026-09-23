using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Bk.Termdat.Business.Resource.Models;

namespace Bk.Termdat.Business.Services.Interfaces;

public interface IWatchlistService
{
    Task<IEnumerable<WatchlistEntry>> GetEntries(IEnumerable<int> entryIds);
    Task<Stream> CreateExportPdf(IEnumerable<int> entryIds);
}
