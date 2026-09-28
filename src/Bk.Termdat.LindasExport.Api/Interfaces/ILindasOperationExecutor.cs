namespace Bk.Termdat.LindasExport.Api.Interfaces;

public interface ILindasOperationExecutor
{
    Task Execute(string target, string operation, Func<Task> action, CancellationToken ct);
}
