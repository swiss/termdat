namespace Bk.Termdat.LindasExport.Api.Models;

public sealed record LindasExportResult(
    IReadOnlyCollection<string> SuccessfulTargets,
    IReadOnlyCollection<string> FailedTargets)
{
    public bool IsSuccess => FailedTargets.Count == 0;
}
