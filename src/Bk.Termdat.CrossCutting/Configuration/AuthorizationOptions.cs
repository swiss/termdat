using System.ComponentModel.DataAnnotations;

namespace Bk.Termdat.CrossCutting.Configuration;

public class AuthorizationOptions
{
    public const string SectionKey = "Authorization";

    [Required]
    public string Allow { get; set; }
    [Required]
    public string Writer { get; set; }
    [Required]
    public string SuperUser { get; set; }
    [Required]
    public string SuperReader { get; set; }
    [Required]
    public string Admin { get; set; }
}
