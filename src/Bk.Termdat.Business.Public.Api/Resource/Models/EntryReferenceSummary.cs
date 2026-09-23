namespace Bk.Termdat.Business.Public.Api.Resource.Models;

/// <summary>
/// Represents a reference to an other entry.
/// </summary>
public class EntryReferenceSummary
{
    /// <summary>
    /// Language dependent text of the reference type ("&lt;leer&gt;", "GEN", "SPE", oder "ANT" for German).
    /// </summary>
    public string Type { get; set; }
    /// <summary>
    /// Text of the referenced entry.
    /// </summary>
    public string Text { get; set; }
    /// <summary>
    /// TERMDAT URL of the referenced entry.
    /// </summary>
    public string Url { get; set; }
}
