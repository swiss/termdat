using System.ComponentModel.DataAnnotations;

namespace Bk.Termdat.LindasExport.Api.Configuration;

public class SparqlTargetsOptions
{
    public const string SectionKey = "SparqlTargets";

    [Required]
    [MinLength(1, ErrorMessage = "At least one SPARQL target must be configured")]
    public required Dictionary<string, SparqlTargetConfiguration> Targets { get; init; }
}

public class SparqlTargetConfiguration
{
    [Required]
    public required string GraphStoreProtocolEndPoint { get; init; }

    [Required]
    public required string HealthEndPoint { get; init; }

    [Required]
    public required int RequestTimeoutMs { get; init; }

    [Required]
    public required string Username { get; init; }

    [Required]
    public required string Password { get; init; }

    [Required]
    public required SparqlProxyOptions Proxy { get; init; }
}
