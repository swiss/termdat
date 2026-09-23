using System.ComponentModel.DataAnnotations;

namespace Bk.Termdat.CrossCutting.Configuration;

public class FileImportOptions
{
    public const string SectionKey = "FileImport";

    [Required]
    public string Email { get; set; }
}
