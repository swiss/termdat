using Bk.Termdat.LindasExport.Api.Configuration;
using Bk.Termdat.LindasExport.Api.Interfaces;
using Microsoft.Extensions.Options;

namespace Bk.Termdat.LindasExport.Api.Services;

public class ExportScheduler : IExportScheduler
{
    private const string TimeZoneId = "Europe/Zurich";
    private readonly LindasExportOptions options;
    private readonly TimeProvider timeProvider;
    private readonly TimeZoneInfo timeZone;

    public ExportScheduler(IOptions<LindasExportOptions> options, TimeProvider timeProvider)
    {
        this.options = options.Value;
        this.timeProvider = timeProvider;
        timeZone = TimeZoneInfo.FindSystemTimeZoneById(TimeZoneId);
    }

    public DateTimeOffset GetNextRun()
    {
        var now = timeProvider.GetUtcNow();
        var localNow = TimeZoneInfo.ConvertTime(now, timeZone);
        var localRun = localNow.Date.Add(options.ExportTime.ToTimeSpan());

        if (localRun <= localNow.DateTime)
        {
            localRun = localRun.AddDays(1);
        }

        // During the spring clock change, some local times don’t exist. IsInvalidTime detects that gap, and the loop moves the scheduled time forward until it’s valid.
        while (timeZone.IsInvalidTime(localRun))
        {
            localRun = localRun.AddMinutes(1);
        }

        // During the autumn clock change, some local times happen twice. IsAmbiguousTime detects this; choosing the larger offset selects the first occurrence, then converts it to UTC.
        if (timeZone.IsAmbiguousTime(localRun))
        {
            var firstOffset = timeZone.GetAmbiguousTimeOffsets(localRun).Max();
            return new DateTimeOffset(localRun, firstOffset).ToUniversalTime();
        }

        return new DateTimeOffset(TimeZoneInfo.ConvertTimeToUtc(localRun, timeZone));
    }
}
