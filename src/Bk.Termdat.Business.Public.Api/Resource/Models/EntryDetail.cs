using System.Collections.Generic;

namespace Bk.Termdat.Business.Public.Api.Resource.Models;

public class EntryDetail
{
    /// <summary>
    /// Entry Id
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Url to the entry in TERMDAT
    /// </summary>
    public string Url { get; set; }

    /// <summary>
    /// Status of the entry.
    /// </summary>
    public CodeAndText Status { get; set; } = new();
    /// <summary>
    /// Reliability of the entry.
    /// </summary>
    public CodeAndText Reliability { get; set; } = new();
    /// <summary>
    /// Office of this entry.
    /// </summary>
    public OfficeSummary Office { get; set; }
    /// <summary>
    /// Assigned collection to this entry.
    /// </summary>
    public CollectionSummary Collection { get; set; }
    /// <summary>
    /// Assigned classification to this entry.
    /// </summary>
    public ClassificationSummary Classification { get; set; }
    /// <summary>
    /// Assigned subjects.
    /// </summary>
    public IList<SubjectSummary> Subject { get; set; }
    /// <summary>
    /// Language dependent synonyms of the entry.
    /// </summary>
    public IList<EntryLanguageDetail> LanguageDetails { get; set; }

    /// <summary>
    /// Search hits within the synonyms. Only provided, when you use "search" API with ReturnType=Summary. Not included for ReturnType=Detail or "entry" API.
    /// </summary>
    public IList<Dictionary<string, string>> Hits { get; set; }
}
