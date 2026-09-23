using System.ComponentModel.DataAnnotations;

namespace Bk.Termdat.LindasExport.Api.Configuration;

public class TermdatGraphOptions
{
    public const string SectionKey = "TermdatGraph";

    [Required]
    public required int ChunkSize { get; init; } = 1_000;
}
