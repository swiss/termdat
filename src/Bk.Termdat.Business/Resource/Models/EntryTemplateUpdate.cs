using System.Collections.Generic;

namespace Bk.Termdat.Business.Resource.Models;

public class EntryTemplateUpdate : IEntryModification
{
    public int Id { get; set; }
    public Status Status { get; set; }
    public Reliability Reliability { get; set; }
    public string Comment { get; set; }
    public int? Identification { get; set; }

    public int OfficeId { get; set; }
    public int CollectionId { get; set; }
    public int? ConceptSystemPositionId { get; set; }

    public IList<int> LenochClassificationIds { get; set; }
    public IList<int> RelatedCollectionIds { get; set; }
    public IList<int> GlossaryIds { get; set; }
    public IList<AdminInfoSummary> AdminInfos { get; set; }
    public IList<EntryModificationLanguageDetail> LanguageDetails { get; set; }
    public IList<EntryModificationReference> References { get; set; }
    public IList<EntryModificationExternalLink> ExternalLinks { get; set; }
    public IList<int> SubjectIds { get; set; }
    public IList<int> DomainIds { get; set; }
}
