using System.ComponentModel.DataAnnotations;

namespace Bk.Termdat.LindasExport.Api.Configuration;

public class LindasExportOptions : IValidatableObject
{
    public const string SectionKey = "LindasExport";

    [Required]
    public bool Enabled { get; init; }

    [Required]
    public bool AddCubeLinkData { get; init; }

    [Required]
    public required TimeOnly ExportTime { get; init; }

    [Required, Range(1, 10)]
    public required int MaximumAttempts { get; init; }

    [Required, Range(0, 5)]
    public required int RetryBaseDelayMinutes { get; init; }

    [Required, Range(0, 10000)]
    public required int MinimumChunkDelayMs { get; init; }

    [Required, Range(0, 10000)]
    public required int MaximumChunkDelayMs { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (MaximumChunkDelayMs < MinimumChunkDelayMs)
        {
            yield return new ValidationResult(
                $"{nameof(MaximumChunkDelayMs)} must be greater than or equal to {nameof(MinimumChunkDelayMs)}.",
                [nameof(MaximumChunkDelayMs), nameof(MinimumChunkDelayMs)]);
        }
    }
}
