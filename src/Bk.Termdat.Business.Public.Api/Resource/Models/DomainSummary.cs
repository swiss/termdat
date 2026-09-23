namespace Bk.Termdat.Business.Public.Api.Resource.Models;

/// <summary>
/// Domain information.
/// Part of the new TERMDAT 3.0 classification system.
/// </summary>
public class DomainSummary
{
    /// <summary>
    /// Id of the domain.
    /// </summary>
    public int Id { get; set; }
    /// <summary>
    /// Language specific text of the domain.
    /// </summary>
    public string Text { get; set; }
}
