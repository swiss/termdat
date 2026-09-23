using System.Collections.Generic;

namespace Bk.Termdat.Business.Domain.Models;

public class Collection : EntityChangeTracking, IMultiLingualEntity<CollectionTranslation>
{
    public string Code { get; set; }
    public string Description { get; set; }
    public string Documentation { get; set; }
    public bool IsObsolete { get; set; }
    public bool Active { get; set; }
    public bool ActiveApi { get; set; }
    public bool Deleted { get; set; }
    public bool ExportLindas { get; set; }

    public int? ClassificationId { get; set; }

    public ConceptSystem ConceptSystem { get; set; }
    public Classification Classification { get; set; }
    public ICollection<CollectionTranslation> Translations { get; set; }
    public ICollection<Entry> Entries { get; set; }
    public ICollection<Entry> RelatedEntries { get; set; }
    public ICollection<UserAccount> UserAccounts { get; set; }
}
