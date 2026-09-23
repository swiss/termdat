using System.ComponentModel.DataAnnotations;

namespace Bk.Termdat.CrossCutting.Configuration;

public class PublicApiOptions
{
    public const string SectionKey = "PublicApi";

    [Required]
    public int[] DefaultOfficeIds { get; set; }
    [Required]
    public string DefaultVariaClassification { get; set; }
    [Required]
    public int MaximumEntryIds { get; set; }
}
