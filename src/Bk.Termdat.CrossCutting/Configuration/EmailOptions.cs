using System.ComponentModel.DataAnnotations;

namespace Bk.Termdat.CrossCutting.Configuration;

public class EmailOptions
{
    public const string SectionKey = "Email";

    [Required]
    public required string SenderAddress { get; init; }
}
