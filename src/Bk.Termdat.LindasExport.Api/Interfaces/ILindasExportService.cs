using Bk.Termdat.LindasExport.Api.Models;

namespace Bk.Termdat.LindasExport.Api.Interfaces;

public interface ILindasExportService
{
    Task<LindasExportResult> ExportCollections(CancellationToken ct);
}
