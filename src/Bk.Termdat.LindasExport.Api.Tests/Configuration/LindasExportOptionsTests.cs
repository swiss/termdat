using System.ComponentModel.DataAnnotations;
using Bk.Termdat.LindasExport.Api.Configuration;

namespace Bk.Termdat.LindasExport.Api.Tests.Configuration;

[TestFixture]
internal class LindasExportOptionsTests
{
    [Test]
    public void Validate_WhenMaximumDelayIsBelowMinimum_ShouldFail()
    {
        var options = new LindasExportOptions
        {
            MinimumChunkDelayMs = 1_000,
            MaximumChunkDelayMs = 500,
            RetryBaseDelayMinutes = 0,
            ExportTime = default,
            MaximumAttempts = 0
        };
        var results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(
            options,
            new ValidationContext(options),
            results,
            true);

        Assert.Multiple(() =>
        {
            Assert.That(isValid, Is.False);
            Assert.That(results, Has.Count.EqualTo(1));
        });
    }
}
