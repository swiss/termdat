using Bk.Termdat.LindasExport.Api.Configuration;
using Bk.Termdat.LindasExport.Api.Interfaces;
using Microsoft.Extensions.Options;

namespace Bk.Termdat.LindasExport.Api.Services;

public class ChunkDelay : IChunkDelay
{
    private readonly LindasExportOptions options;
    private readonly TimeProvider timeProvider;

    public ChunkDelay(IOptions<LindasExportOptions> options, TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(options);

        this.options = options.Value;
        this.timeProvider = timeProvider;
    }

    public Task Wait(CancellationToken ct)
    {
        #pragma warning disable CA5394 // Non-security-related random delay
        var milliseconds = Random.Shared.Next(options.MinimumChunkDelayMs, options.MaximumChunkDelayMs + 1);
        return Task.Delay(TimeSpan.FromMilliseconds(milliseconds), timeProvider, ct);
        #pragma warning restore CA5394
    }
}
