namespace Bk.Termdat.Business.Domain.Models;

public class EntryReference
{
    public int Id { get; set; }
    public int EntryId { get; set; }
    public ReferenceType Type { get; set; }
    public int ReferencedEntryId { get; set; }
    public Entry Entry { get; set; }
    public Entry ReferencedEntry { get; set; }
}
