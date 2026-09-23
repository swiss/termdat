using System;
using Bk.Termdat.CrossCutting;

namespace Bk.Termdat.Private.Api.HostedServices;

public class BackgroundServiceScheduler : IBackgroundServiceScheduler
{
    private readonly IDateTimeProvider dateTimeProvider;

    public BackgroundServiceScheduler(IDateTimeProvider dateTimeProvider)
    {
        this.dateTimeProvider = dateTimeProvider;
    }

    public TimeSpan GetNextRun(int interval)
    {
        var now = dateTimeProvider.GetNow();
        var nextRunTime = now.AddSeconds(interval);
        return nextRunTime.Subtract(now);
    }
}
