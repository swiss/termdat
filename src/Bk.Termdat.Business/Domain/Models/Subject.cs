using System.Collections.Generic;

namespace Bk.Termdat.Business.Domain.Models;

// Subject - Fachgebiet
public class Subject : EntityChangeTracking, IMultiLingualEntity<SubjectTranslation>
{
    public ICollection<SubjectTranslation> Translations { get; set; }
    public ICollection<Domain> Domains { get; set; }
    public ICollection<Entry> Entries { get; set; }
    public ICollection<LenochClassificationSubjectDomainMapping> LenochClassificationSubjectDomainMappings { get; set; }
}
