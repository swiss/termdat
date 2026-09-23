using System.ComponentModel.DataAnnotations;

namespace Bk.Termdat.Gateway.Configuration;

public class GatewayOptions
{
    public const string SectionKey = "Gateway";

    [Required]
    public required string UserEndpoint { get; init; }

    [Required]
    public required string LoginEndpoint { get; init; }

    [Required]
    public required string LogoutEndpoint { get; init; }
}
