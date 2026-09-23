using System.ComponentModel.DataAnnotations;

namespace Bk.Termdat.CrossCutting.Configuration;

public class AuthenticationOptions
{
    public const string SectionKey = "Authentication";

    [Required]
    public string Url { get; set; }
    [Required]
    public string ClientId { get; set; }
    [Required]
    public bool IsHttps { get; set; }
}
