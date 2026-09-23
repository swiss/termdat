using System.ComponentModel.DataAnnotations;

namespace Bk.Termdat.Gateway.Configuration;

public class CookieOptions
{
    public const string SectionKey = "Cookie";

    [Required]
    public required string Name { get; init; }

    [Required]
    public bool HttpOnly { get; init; }

    [Required]
    public required string SecurePolicy { get; init; }

    [Required]
    public required string SameSite { get; init; }

    [Required]
    public int ExpireTimeSpanHours { get; init; }

    [Required]
    public bool SlidingExpiration { get; init; }
}
