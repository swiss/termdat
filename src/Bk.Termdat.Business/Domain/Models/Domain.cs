using System.Collections.Generic;

namespace Bk.Termdat.Business.Domain.Models;

// Domain - Sachgebiet
public class Domain : EntityChangeTracking, IMultiLingualEntity<DomainTranslation>
{
    public int SubjectId { get; set; }
    public Subject Subject { get; set; }
    public ICollection<DomainTranslation> Translations { get; set; }
    public ICollection<Entry> Entries { get; set; }
    public ICollection<LenochClassificationSubjectDomainMapping> LenochClassificationSubjectDomainMappings { get; set; }
}
