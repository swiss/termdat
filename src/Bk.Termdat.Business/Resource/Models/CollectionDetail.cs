namespace Bk.Termdat.Business.Resource.Models;

public class CollectionDetail : CollectionSummary
{
    public ConceptSystemSummary ConceptSystem { get; set; }
    public int EntryCount { get; set; }
}
