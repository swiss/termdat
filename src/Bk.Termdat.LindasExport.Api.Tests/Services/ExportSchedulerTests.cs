using Bk.Termdat.LindasExport.Api.Configuration;
using Bk.Termdat.LindasExport.Api.Services;
using Microsoft.Extensions.Options;

namespace Bk.Termdat.LindasExport.Api.Tests.Services;

[TestFixture]
internal class ExportSchedulerTests
{
    [TestCase("2026-01-15T00:00:00Z", "2026-01-15T01:00:00Z")]
    [TestCase("2026-01-15T02:00:00Z", "2026-01-16T01:00:00Z")]
    public void GetNextRun_ShouldUseNextEuropeZurichTime(string now, string expected)
    {
        var scheduler = CreateScheduler(DateTimeOffset.Parse(now), new TimeOnly(2, 0));

        var result = scheduler.GetNextRun();

        Assert.That(result, Is.EqualTo(DateTimeOffset.Parse(expected)));
    }

    [Test]
    public void GetNextRun_ForInvalidDstTime_ShouldUseFirstValidTime()
    {
        var scheduler = CreateScheduler(
            DateTimeOffset.Parse("2026-03-29T00:00:00Z"),
            new TimeOnly(2, 30));

        var result = scheduler.GetNextRun();

        Assert.That(result, Is.EqualTo(DateTimeOffset.Parse("2026-03-29T01:00:00Z")));
    }

    [Test]
    public void GetNextRun_ForAmbiguousDstTime_ShouldUseFirstOccurrence()
    {
        var scheduler = CreateScheduler(
            DateTimeOffset.Parse("2026-10-25T00:00:00Z"),
            new TimeOnly(2, 30));

        var result = scheduler.GetNextRun();

        Assert.That(result, Is.EqualTo(DateTimeOffset.Parse("2026-10-25T00:30:00Z")));
    }

    private static ExportScheduler CreateScheduler(DateTimeOffset now, TimeOnly exportTime)
    {
        var options = Options.Create(new LindasExportOptions
        {
            ExportTime = exportTime,
            RetryBaseDelayMinutes = 0,
            MinimumChunkDelayMs = 0,
            MaximumChunkDelayMs = 0,
            MaximumAttempts = 0
        });
        return new ExportScheduler(options, new FixedTimeProvider(now));
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
