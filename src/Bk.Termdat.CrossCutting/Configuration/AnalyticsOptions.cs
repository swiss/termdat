using System.ComponentModel.DataAnnotations;

namespace Bk.Termdat.CrossCutting.Configuration;

public class AnalyticsOptions
{
    public const string SectionKey = "Analytics";

    [Required]
    public bool Enabled { get; init; }

    [Required]
    public required string Url { get; init; }

    [Required]
    public required string SiteId { get; init; }
}
