using Collection = Bk.Termdat.LindasExport.Api.Models.Collection;

namespace Bk.Termdat.LindasExport.Api.Interfaces;

public interface ITermdatGraphService
{
    Task DeleteGraph(CancellationToken ct);
    Task AddCollection(Collection collection, CancellationToken ct);
    Task AddDataSet(CancellationToken ct);
}
