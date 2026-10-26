using System.Collections.Generic;

namespace Bk.Termdat.Business.Domain.Models;

// AdminInfo - Verwaltungsangaben
public class AdminInfo : EntityChangeTracking
{
    public string Code { get; set; }

    public string? Description { get; set; }

    public ICollection<Entry> Entries { get; set; }
}
