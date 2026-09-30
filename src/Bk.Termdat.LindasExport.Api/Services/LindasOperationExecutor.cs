using Bk.Termdat.LindasExport.Api.Configuration;
using Bk.Termdat.LindasExport.Api.Interfaces;
using Microsoft.Extensions.Options;

namespace Bk.Termdat.LindasExport.Api.Services;

public class LindasOperationExecutor : ILindasOperationExecutor
{
    private readonly ILogger<LindasOperationExecutor> logger;
    private readonly LindasExportOptions options;
    private readonly TimeProvider timeProvider;

    public LindasOperationExecutor(
        ILogger<LindasOperationExecutor> logger,
        IOptions<LindasExportOptions> options,
        TimeProvider timeProvider)
    {
        this.logger = logger;
        this.options = options.Value;
        this.timeProvider = timeProvider;
    }

    public async Task Execute(string target, string operation, Func<Task> action, CancellationToken ct)
    {
        for (var attempt = 1; ; attempt++)
        {
            ct.ThrowIfCancellationRequested();

            try
            {
                await action();
                return;
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex) when (attempt < options.MaximumAttempts)
            {
                var delay = TimeSpan.FromMinutes(options.RetryBaseDelayMinutes * attempt * attempt);

                logger.LogWarning(
                    ex,
                    "LINDAS operation failure. Target={Target} Operation={Operation} Attempt={Attempt}/{MaximumAttempts}. Retrying in {Delay}",
                    target, operation, attempt, options.MaximumAttempts, delay);

                await Task.Delay(delay, timeProvider, ct);
            }
            catch (OperationCanceledException ex)
            {
                throw new HttpRequestException(
                    $"LINDAS operation {operation} on target {target} timed out after {attempt} attempts.",
                    ex);
            }
        }
    }
}
