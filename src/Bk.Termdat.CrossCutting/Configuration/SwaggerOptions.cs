using System.ComponentModel.DataAnnotations;

namespace Bk.Termdat.CrossCutting.Configuration;

public class SwaggerOptions
{
    public const string SectionKey = "Swagger";

    [Required]
    public bool? Enabled { get; set; }
}
