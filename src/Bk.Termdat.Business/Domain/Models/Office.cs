using System.Collections.Generic;

namespace Bk.Termdat.Business.Domain.Models;

#pragma warning disable CA1724 // Office is a domain entity name.
public class Office : EntityChangeTracking, IMultiLingualEntity<OfficeTranslation>
{
#pragma warning restore CA1724
    public string Code { get; set; }

    public ICollection<OfficeTranslation> Translations { get; set; }

    public ICollection<Entry> Entries { get; set; }
}
