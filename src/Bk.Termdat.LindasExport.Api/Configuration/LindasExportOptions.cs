using System.ComponentModel.DataAnnotations;

namespace Bk.Termdat.LindasExport.Api.Configuration;

public class LindasExportOptions
{
    public const string SectionKey = "LindasExport";

    [Required]
    public bool Enabled { get; init; }
    [Required]
    public bool AddCubeLinkData { get; init; }
}
