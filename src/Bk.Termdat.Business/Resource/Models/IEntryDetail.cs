using System;
using System.Collections.Generic;

namespace Bk.Termdat.Business.Resource.Models;

public interface IEntryDetail
{
    int Id { get; set; }
    Status Status { get; set; }
    Reliability Reliability { get; set; }
    string Comment { get; set; }
    int? Identification { get; set; }
    OfficeSummary Office { get; set; }
    CollectionSummary Collection { get; set; }
    ConceptSystemPositionSummary ConceptSystemPosition { get; set; }
    PictureDetail Picture { get; set; }
    IList<LenochClassificationSummary> LenochClassifications { get; set; }
    IList<SubjectSummary> Subjects { get; set; }
    IList<DomainSummary> Domains { get; set; }
    IList<CollectionSummary> RelatedCollections { get; set; }
    IList<GlossarySummary> Glossaries { get; set; }
    IList<AdminInfoSummary> AdminInfos { get; set; }
    IList<EntryExternalLinkSummary> ExternalLinkSummary { get; set; }
    bool Deleted { get; set; }
    uint ConcurrencyToken { get; set; }
    bool CanEdit { get; set; }
    bool CanEditCollection { get; set; }
    bool CanDelete { get; set; }
    bool CanRestore { get; set; }
    DateTime? Locked { get; set; }
    string LockedBy { get; set; }
}

public interface IEntryDetail<T> : IEntryDetail
    where T : IEntryLanguageDetail
{
    IList<T> LanguageDetails { get; set; }
}
