namespace Bk.Termdat.LindasExport.Api.Interfaces;

public interface IExportScheduler
{
    DateTimeOffset GetNextRun();
}
