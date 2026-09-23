using System.ComponentModel.DataAnnotations;

namespace Bk.Termdat.CrossCutting.Configuration;

public class SearchQueueOptions
{
    public const string SectionKey = "SearchQueue";

    [Required]
    public int PublishTimeoutInSeconds { get; set; }
}
