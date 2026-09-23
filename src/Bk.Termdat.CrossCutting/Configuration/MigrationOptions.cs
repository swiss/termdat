using System.ComponentModel.DataAnnotations;

namespace Bk.Termdat.CrossCutting.Configuration;

public class MigrationOptions
{
    public const string SectionKey = "Migration";

    [Required]
    public bool IsAutoMigration { get; set; }
}
