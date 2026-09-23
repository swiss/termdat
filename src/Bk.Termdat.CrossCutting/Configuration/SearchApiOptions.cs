using System.ComponentModel.DataAnnotations;

namespace Bk.Termdat.CrossCutting.Configuration;

public class SearchApiOptions
{
    public const string SectionKey = "SearchApi";

    public const string HttpClientName = "search.api";

    [Required]
    public string BaseUrl { get; set; }
    [Required]
    public string UrnSearch { get; set; }
    [Required]
    public string UrnRecommend { get; set; }
}
