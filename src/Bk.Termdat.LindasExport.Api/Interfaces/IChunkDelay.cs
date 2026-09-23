namespace Bk.Termdat.LindasExport.Api.Interfaces;

public interface IChunkDelay
{
    Task Wait(CancellationToken ct);
}
