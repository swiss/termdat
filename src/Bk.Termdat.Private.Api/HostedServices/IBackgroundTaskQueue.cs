using System;
using System.Threading;
using System.Threading.Tasks;

namespace Bk.Termdat.Private.Api.HostedServices;

public interface IBackgroundTaskQueue
{
    ValueTask QueueBackgroundWorkItemAsync(Func<CancellationToken, IServiceProvider, Task> workItem);

    ValueTask<Func<CancellationToken, IServiceProvider, Task>> DequeueBackgroundWorkItemAsync(
        CancellationToken cancellationToken);
}
