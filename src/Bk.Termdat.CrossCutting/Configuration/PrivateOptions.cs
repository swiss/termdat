using System.ComponentModel.DataAnnotations;

namespace Bk.Termdat.CrossCutting.Configuration;

public class PrivateOptions
{
    public const string SectionKey = "Private";
    [Required]
    public int AchOfficeId { get; set; }
    [Required]
    public string ConceptUriFormat { get; set; }
}
