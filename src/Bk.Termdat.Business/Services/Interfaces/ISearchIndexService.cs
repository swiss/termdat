using System.Threading;
using System.Threading.Tasks;

namespace Bk.Termdat.Business.Services.Interfaces;

public interface ISearchIndexService
{
    Task CreateIndex(CancellationToken stoppingToken);
}
