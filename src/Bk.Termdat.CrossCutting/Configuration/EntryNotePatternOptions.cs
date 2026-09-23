using System.ComponentModel.DataAnnotations;

namespace Bk.Termdat.CrossCutting.Configuration;

public class EntryNotePatternOptions
{
    public const string SectionKey = "EntryNotePattern";
    [Required]
    public string StartingPattern { get; set; }
    [Required]
    public string ObsoletePattern { get; set; }
    [Required]
    public string RarePattern { get; set; }
    [Required]
    public string ShouldAvoidPattern { get; set; }
}
