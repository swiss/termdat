
using System.ComponentModel.DataAnnotations;

namespace Bk.Termdat.CrossCutting.Configuration;

public class StatisticDeleteOptions
{
    public const string SectionKey = "StatisticDelete";

    [Required]
    public int IntervalInSeconds { get; set; }
}
