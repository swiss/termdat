using System.Collections.Generic;

namespace Bk.Termdat.Business.Resource.Models;

public class EntryReferencedByDetail : IEntryReferenceDetail
{
    public EntryReferencedByDetail()
    {
        Languages = new List<EntryReferenceDetailLanguage>();
    }

    public int EntryId { get; set; }
    public ReferenceType? Type { get; set; }
    public IList<EntryReferenceDetailLanguage> Languages { get; set; }
}
