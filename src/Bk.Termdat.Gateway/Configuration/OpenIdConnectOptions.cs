using System.ComponentModel.DataAnnotations;

namespace Bk.Termdat.Gateway.Configuration;

public class OpenIdConnectOptions
{
    public const string SectionKey = "OpenIdConnect";

    [Required]
    public required string Authority { get; init; }

    [Required]
    public required string ClientId { get; init; }

    [Required]
    public required string ClientSecret { get; init; }

    [Required]
    public required string CallbackPath { get; init; }

    [Required]
    public required string SignedOutCallbackPath { get; init; }

    [Required]
    public required string ResponseType { get; init; }

    [Required]
    public bool SaveTokens { get; init; }

    [Required]
    public bool RequireHttpsMetadata { get; init; }

    [Required]
    public required string TokenEndpointPath { get; init; }

    [Required]
    public required string Scope { get; init; }
}
