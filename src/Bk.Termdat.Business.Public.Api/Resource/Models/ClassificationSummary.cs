namespace Bk.Termdat.Business.Public.Api.Resource.Models;

/// <summary>
/// Classification information
/// </summary>
public class ClassificationSummary
{
    /// <summary>
    /// Id of the classification
    /// </summary>
    public int Id { get; set; }
    /// <summary>
    /// Language independent code of the classification.
    /// </summary>
    public string Code { get; set; }
    /// <summary>
    /// Language specific text of the classification.
    /// </summary>
    public string Text { get; set; }
}
