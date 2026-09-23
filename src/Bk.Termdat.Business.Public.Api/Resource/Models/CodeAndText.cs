namespace Bk.Termdat.Business.Public.Api.Resource.Models;

/// <summary>
/// Simple code and text representation.
/// </summary>
public class CodeAndText
{
    /// <summary>
    /// Language independent Code.
    /// </summary>
    public string Code { get; set; }
    /// <summary>
    /// Language dependent text.
    /// </summary>
    public string Text { get; set; }
}
