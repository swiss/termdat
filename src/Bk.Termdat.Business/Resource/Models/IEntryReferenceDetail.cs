using System.Collections.Generic;

namespace Bk.Termdat.Business.Resource.Models;

public interface IEntryReferenceDetail
{
    int EntryId { get; set; }
    ReferenceType? Type { get; set; }
    IList<EntryReferenceDetailLanguage> Languages { get; set; }
}
