using System.Collections.Generic;

namespace Bk.Termdat.Business.Resource.Models;

public interface IEntryModification
{
    Status Status { get; set; }
    Reliability Reliability { get; set; }
    string Comment { get; set; }
    int? Identification { get; set; }

    int OfficeId { get; set; }
    int CollectionId { get; set; }
    int? ConceptSystemPositionId { get; set; }

    IList<int> LenochClassificationIds { get; set; }
    IList<int> SubjectIds { get; set; }
    IList<int> DomainIds { get; set; }
    IList<int> RelatedCollectionIds { get; set; }
    IList<int> GlossaryIds { get; set; }
    IList<AdminInfoSummary> AdminInfos { get; set; }
    IList<EntryModificationLanguageDetail> LanguageDetails { get; set; }
    IList<EntryModificationReference> References { get; set; }
    IList<EntryModificationExternalLink> ExternalLinks { get; set; }
}
