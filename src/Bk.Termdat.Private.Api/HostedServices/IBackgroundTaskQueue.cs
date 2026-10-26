using System;
using System.Threading;
using System.Threading.Tasks;

namespace Bk.Termdat.Private.Api.HostedServices;

#pragma warning disable CA1711
public interface IBackgroundTaskQueue
{
#pragma warning restore CA1711
    ValueTask QueueBackgroundWorkItemAsync(Func<CancellationToken, IServiceProvider, Task> workItem);

    ValueTask<Func<CancellationToken, IServiceProvider, Task>> DequeueBackgroundWorkItemAsync(
        CancellationToken cancellationToken);
}
