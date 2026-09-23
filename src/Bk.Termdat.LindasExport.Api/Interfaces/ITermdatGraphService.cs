using Collection = Bk.Termdat.LindasExport.Api.Models.Collection;

namespace Bk.Termdat.LindasExport.Api.Interfaces;

public interface ITermdatGraphService
{
    IReadOnlyCollection<string> Targets { get; }
    Task DeleteGraph(string target, CancellationToken ct);
    Task AddCollection(string target, Collection collection, CancellationToken ct);
    Task AddDataSet(string target, CancellationToken ct);
}
