namespace Bk.Termdat.Business.Public.Api.Resource.Models;

/// <summary>
/// Collection information
/// </summary>
public class CollectionSummary
{
    /// <summary>
    /// Id of the collection.
    /// </summary>
    public int Id { get; set; }
    /// <summary>
    /// Language independent code of the collection.
    /// </summary>
    public string Code { get; set; }
    /// <summary>
    /// Language specific text of the collection.
    /// </summary>
    public string Text { get; set; }
}
