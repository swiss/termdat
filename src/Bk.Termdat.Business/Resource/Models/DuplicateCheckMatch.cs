namespace Bk.Termdat.Business.Resource.Models;

public class DuplicateCheckMatch
{
    public int EntryId { get; init; }
    public int[] MatchedEntryIds { get; init; } = [];
    public string[] MatchedKeys { get; init; } = [];
}
