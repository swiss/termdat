using System;
using System.Collections.Generic;

namespace Bk.Termdat.Business.Domain.Models;

public class Entry : EntityChangeTracking, IMultiLingualEntity<EntryLanguage>
{
    public Status Status { get; set; }
    public Reliability Reliability { get; set; }
    public int? Identification { get; set; }
    public string? Comment { get; set; }
    public string? AdminInfo { get; set; }

    public bool Deleted { get; set; }

    public int OfficeId { get; set; }
    public int CollectionId { get; set; }
    public int? ConceptSystemPositionId { get; set; }

    public DateTime? Locked { get; set; }
    public string? LockedBy { get; set; }

    public Collection Collection { get; set; }
    public Office Office { get; set; }
    public Picture Picture { get; set; }
    public ConceptSystemPosition ConceptSystemPosition { get; set; }
    public ICollection<Subject> Subjects { get; set; }
    public ICollection<Domain> Domains { get; set; }
    public ICollection<Collection> RelatedCollections { get; set; }
    public ICollection<LenochClassification> LenochClassifications { get; set; }
    public ICollection<Glossary> Glossaries { get; set; }
    public ICollection<EntryLanguage> Translations { get; set; }
    public ICollection<EntryReference> ReferencedByEntries { get; set; }
    public ICollection<EntryReference> References { get; set; }
    public ICollection<EntryExternalLink> ExternalLinks { get; set; }
    public ICollection<AdminInfo> AdminInfos { get; set; }

    //Note: Contains the ID provided by the TBX import and originally generated in the Star software.
    public string ExternalSystemId { get; set; }
}
