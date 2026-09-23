namespace Bk.Termdat.Business.Public.Api.Resource.Models;

/// <summary>
/// Return type for entry search.
/// </summary>
public enum ReturnType
{
    /// <summary>
    /// Returns a complete search result including all entries and their synonyms.
    /// </summary>
    Detail,
    /// <summary>
    /// Returns only the entries with their metadata and the hits.
    /// </summary>
    Summary
}
