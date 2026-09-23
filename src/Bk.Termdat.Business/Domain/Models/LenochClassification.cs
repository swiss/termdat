using System.Collections.Generic;

namespace Bk.Termdat.Business.Domain.Models;

public class LenochClassification : EntityChangeTracking, IMultiLingualEntity<LenochClassificationTranslation>
{
    public string Code { get; set; }
    public ICollection<Entry> Entries { get; set; }
    public ICollection<LenochClassificationTranslation> Translations { get; set; }
    public ICollection<LenochClassificationSubjectDomainMapping> LenochClassificationSubjectDomainMappings { get; set; }
}
