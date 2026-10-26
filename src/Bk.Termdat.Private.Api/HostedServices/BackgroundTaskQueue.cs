using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Bk.Termdat.Private.Api.HostedServices;

#pragma warning disable CA1711
public class BackgroundTaskQueue : IBackgroundTaskQueue
{
#pragma warning restore CA1711
    private readonly Channel<Func<CancellationToken, IServiceProvider, Task>> queue;

    public BackgroundTaskQueue(int capacity)
    {
        // Note:
        // The capacity should be set based on the expected application load and the number of concurrent threads accessing the queue.
        // BoundedChannelFullMode.Wait will cause calls to WriteAsync() to return a task that will not complete until space becomes available.
        // This leads to backpressure if too many publishers/calls start to accumulate.
        var options = new BoundedChannelOptions(capacity) { FullMode = BoundedChannelFullMode.Wait };
        queue = Channel.CreateBounded<Func<CancellationToken, IServiceProvider, Task>>(options);
    }

    public async ValueTask QueueBackgroundWorkItemAsync(Func<CancellationToken, IServiceProvider, Task> workItem)
    {
        ArgumentNullException.ThrowIfNull(workItem);

        await queue.Writer.WriteAsync(workItem);
    }

    public async ValueTask<Func<CancellationToken, IServiceProvider, Task>> DequeueBackgroundWorkItemAsync(
        CancellationToken cancellationToken)
    {
        return await queue.Reader.ReadAsync(cancellationToken);
    }
}
