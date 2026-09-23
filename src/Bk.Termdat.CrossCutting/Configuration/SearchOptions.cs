using System.ComponentModel.DataAnnotations;

namespace Bk.Termdat.CrossCutting.Configuration;

public class SearchOptions
{
    public const string SectionKey = "Search";
    [Required]
    public bool? ContinueOnError { get; set; }
    [Required]
    public float RefreshDelay { get; set; }
    [Required]
    public string PreferredEntrySequenceId { get; set; }
    [Required]
    public int AchOfficeId { get; set; }
    [Required]
    public float QueryBoostPriorityMultiplier { get; set; }
    [Required]
    public float QueryBoostHighestPriority { get; set; }
    [Required]
    public float QueryBoostVeryHighPriority { get; set; }
    [Required]
    public float QueryBoostHighPriority { get; set; }
    [Required]
    public float QueryBoostMidPriority { get; set; }
    [Required]
    public float QueryBoostLowPriority { get; set; }
    [Required]
    public float QueryBoostVeryLowPriority { get; set; }
    [Required]
    public float QueryBoostLowestPriority { get; set; }
    [Required]
    public int RecommendationMinLength { get; set; }
    [Required]
    public int RecommendationResultSize { get; set; }
    [Required]
    public bool RecreateIndexOnStartUp { get; set; }
    [Required]
    public int IndexPageSize { get; set; }
}
