using System.ComponentModel.DataAnnotations;

namespace Bk.Termdat.LindasExport.Api.Configuration;

public class SparqlProxyOptions
{
    [Required]
    public required string Address { get; init; }

    [Required]
    public bool UseProxy { get; init; }
}
