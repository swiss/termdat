using System.Collections.Generic;

namespace Bk.Termdat.Business.Resource.Models;

public class EntrySummary
{
    public int Id { get; set; }
    public Status Status { get; set; }
    public bool Deleted { get; set; }
    public bool IsMainOffice { get; set; }
    public CollectionSummary Collection { get; set; }
    public ICollection<EntryLanguageSummary> Terms { get; set; }
    public bool CanBeAddedToWatchList { get; set; }
}
