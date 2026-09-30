using Bk.Termdat.LindasExport.Api.Configuration;
using Bk.Termdat.LindasExport.Api.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Bk.Termdat.LindasExport.Api.Tests.Services;

[TestFixture]
internal class LindasOperationExecutorTests
{
    [Test]
    public async Task Execute_ForFailures_ShouldUseQuadraticBackoff()
    {
        var timeProvider = new ImmediateTimeProvider();
        var executor = CreateExecutor(timeProvider);
        var calls = 0;

        await executor.Execute("target", "operation", () =>
        {
            calls++;
            return calls < 5
                ? Task.FromException(new InvalidOperationException())
                : Task.CompletedTask;
        }, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(calls, Is.EqualTo(5));
            Assert.That(timeProvider.Delays, Is.EqualTo(new[]
            {
                TimeSpan.FromMinutes(1),
                TimeSpan.FromMinutes(4),
                TimeSpan.FromMinutes(9),
                TimeSpan.FromMinutes(16)
            }));
        });
    }

    [Test]
    public async Task Execute_WhenOperationTimesOut_ShouldRetry()
    {
        var executor = CreateExecutor(new ImmediateTimeProvider());
        var calls = 0;

        await executor.Execute("target", "operation", () =>
        {
            calls++;
            return calls == 1
                ? Task.FromException(new TaskCanceledException())
                : Task.CompletedTask;
        }, CancellationToken.None);

        Assert.That(calls, Is.EqualTo(2));
    }

    [Test]
    public void Execute_WhenTimeoutAttemptsExhausted_ShouldThrowNonCancellationFailure()
    {
        var executor = CreateExecutor(new ImmediateTimeProvider());
        var calls = 0;

        Assert.ThrowsAsync<HttpRequestException>(() =>
            executor.Execute("target", "operation", () =>
            {
                calls++;
                return Task.FromException(new TaskCanceledException());
            }, CancellationToken.None));

        Assert.That(calls, Is.EqualTo(5));
    }

    [Test]
    public void Execute_WhenAttemptsExhausted_ShouldThrow()
    {
        var executor = CreateExecutor(new ImmediateTimeProvider());
        var calls = 0;

        var exception = Assert.ThrowsAsync<InvalidOperationException>(() =>
            executor.Execute("target", "operation", () =>
            {
                calls++;
                return Task.FromException(new InvalidOperationException());
            }, CancellationToken.None));

        Assert.Multiple(() =>
        {
            Assert.That(exception, Is.Not.Null);
            Assert.That(calls, Is.EqualTo(5));
        });
    }

    [Test]
    public void Execute_ForCancellation_ShouldNotInvokeOperation()
    {
        var executor = CreateExecutor(new ImmediateTimeProvider());
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var calls = 0;

        Assert.ThrowsAsync<OperationCanceledException>(() =>
            executor.Execute("target", "operation", () =>
            {
                calls++;
                return Task.CompletedTask;
            }, cancellation.Token));

        Assert.That(calls, Is.Zero);
    }

    [Test]
    public void Execute_WhenCallerCancelsDuringOperation_ShouldNotRetry()
    {
        var executor = CreateExecutor(new ImmediateTimeProvider());
        using var cancellation = new CancellationTokenSource();
        var calls = 0;

        Assert.ThrowsAsync<TaskCanceledException>(() =>
            executor.Execute("target", "operation", () =>
            {
                calls++;
                cancellation.Cancel();
                return Task.FromCanceled(cancellation.Token);
            }, cancellation.Token));

        Assert.That(calls, Is.EqualTo(1));
    }

    private static LindasOperationExecutor CreateExecutor(TimeProvider timeProvider)
    {
        var options = Options.Create(new LindasExportOptions
        {
            MaximumAttempts = 5,
            RetryBaseDelayMinutes = 1,
            MinimumChunkDelayMs = 0,
            MaximumChunkDelayMs = 0,
            ExportTime = default
        });
        return new LindasOperationExecutor(
            NullLogger<LindasOperationExecutor>.Instance, options, timeProvider);
    }

    private sealed class ImmediateTimeProvider : TimeProvider
    {
        public List<TimeSpan> Delays { get; } = [];

        public override ITimer CreateTimer(
            TimerCallback callback,
            object? state,
            TimeSpan dueTime,
            TimeSpan period)
        {
            Delays.Add(dueTime);
            ThreadPool.QueueUserWorkItem(_ => callback(state));
            return new NoopTimer();
        }
    }

    private sealed class NoopTimer : ITimer
    {
        public bool Change(TimeSpan dueTime, TimeSpan period) => true;
        public void Dispose()
        {
        }

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}
