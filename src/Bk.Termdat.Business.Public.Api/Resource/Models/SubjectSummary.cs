using System.Collections.Generic;

namespace Bk.Termdat.Business.Public.Api.Resource.Models;

/// <summary>
/// Subject information.
/// Part of the new TERMDAT 3.0 classification system.
/// </summary>
public class SubjectSummary
{
    /// <summary>
    /// Id of the classification
    /// </summary>
    public int Id { get; set; }
    /// <summary>
    /// Language specific text of the classification.
    /// </summary>
    public string Text { get; set; }

    public IList<DomainSummary> Domains { get; set; }
}
