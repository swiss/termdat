using System.ComponentModel.DataAnnotations;

namespace Bk.Termdat.CrossCutting.Configuration;

public class TermdatClientOptions
{
    public const string SectionKey = "TermdatClient";

    [Required]
    public string BaseUrl { get; set; }
    [Required]
    public string EntryDetailPath { get; set; }
    [Required]
    public string EntryIdSearchQueryParameter { get; set; }
}
