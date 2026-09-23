using System.ComponentModel.DataAnnotations;

namespace Bk.Termdat.CrossCutting.Configuration;

public class AutoMapperOptions
{
    public const string SectionKey = "AutoMapper";

    [Required]
    public required string LicenseKey { get; set; }
}
