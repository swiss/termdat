namespace Bk.Termdat.Business.Resource.Models;

public class WatchlistEntry
{
    public int EntryId { get; set; }
    public int? Identification { get; set; }
    public ConceptSystemPositionSummary ConceptSystemPosition { get; set; }
    public string Value { get; set; }
    public Status Status { get; set; }
    public bool Deleted { get; set; }
    public bool Locked { get; set; }
    public CollectionSummary Collection { get; set; }
    public bool IsMainOffice { get; set; }
    public string Glossaries { get; set; }
    public string AdminInfos { get; set; }
}
