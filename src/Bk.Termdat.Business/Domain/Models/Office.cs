using System.Collections.Generic;

namespace Bk.Termdat.Business.Domain.Models;

public class Office : EntityChangeTracking, IMultiLingualEntity<OfficeTranslation>
{
    public string Code { get; set; }

    public ICollection<OfficeTranslation> Translations { get; set; }

    public ICollection<Entry> Entries { get; set; }
}
