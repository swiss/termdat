using System.Collections.Generic;

namespace Bk.Termdat.Business.Resource.Models;

public class EntryReferenceDetail : IEntryReferenceDetail
{
    public EntryReferenceDetail()
    {
        Languages = new List<EntryReferenceDetailLanguage>();
    }

    public int Id { get; set; }
    public int EntryId { get; set; }
    public ReferenceType? Type { get; set; }
    public IList<EntryReferenceDetailLanguage> Languages { get; set; }
}
