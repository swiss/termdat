namespace Bk.Termdat.LindasExport.Api.Interfaces;

public interface ILindasExportService
{
    Task ExportCollections(CancellationToken ct);
}
