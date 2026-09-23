using System;
using System.Collections.Generic;

namespace Bk.Termdat.Business.Resource.Models;

public class EntryDetail : IEntryDetail<EntryLanguageDetail>
{
    public int Id { get; set; }
    public string ConceptUri { get; set; }
    public Status Status { get; set; }
    public Reliability Reliability { get; set; }
    public string Comment { get; set; }
    public int? Identification { get; set; }

    public OfficeSummary Office { get; set; }
    public bool IsMainOffice { get; set; }
    public CollectionSummary Collection { get; set; }
    public ConceptSystemPositionSummary ConceptSystemPosition { get; set; }
    public PictureDetail Picture { get; set; }
    public IList<EntryLanguageDetail> LanguageDetails { get; set; }
    public IList<LenochClassificationSummary> LenochClassifications { get; set; }
    public IList<SubjectSummary> Subjects { get; set; }
    public IList<DomainSummary> Domains { get; set; }
    public IList<CollectionSummary> RelatedCollections { get; set; }
    public IList<GlossarySummary> Glossaries { get; set; }
    public IList<EntryReferencedByDetail> ReferencedBy { get; set; }
    public IList<AdminInfoSummary> AdminInfos { get; set; }
    public IList<EntryExternalLinkSummary> ExternalLinkSummary { get; set; }

    public DateTime Created { get; set; }
    public string CreatedBy { get; set; }
    public DateTime Modified { get; set; }
    public string ModifiedBy { get; set; }
    public DateTime? Locked { get; set; }
    public string LockedBy { get; set; }
    public bool Deleted { get; set; }

    public bool CanEdit { get; set; }
    public bool CanEditCollection { get; set; }
    public bool CanDelete { get; set; }
    public bool CanRestore { get; set; }

    public uint ConcurrencyToken { get; set; }
}
