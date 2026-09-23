using System;

namespace Bk.Termdat.Private.Api.HostedServices;

public interface IBackgroundServiceScheduler
{
    TimeSpan GetNextRun(int interval);
}
