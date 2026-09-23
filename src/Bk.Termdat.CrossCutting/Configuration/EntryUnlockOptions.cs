
using System.ComponentModel.DataAnnotations;

namespace Bk.Termdat.CrossCutting.Configuration;

public class EntryUnlockOptions
{
    public const string SectionKey = "EntryUnlock";

    [Required]
    public int IntervalInSeconds { get; set; }
}
