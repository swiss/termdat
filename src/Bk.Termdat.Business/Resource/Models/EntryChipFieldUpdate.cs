using System.Collections.Generic;

namespace Bk.Termdat.Business.Resource.Models;

public class EntryChipFieldUpdate
{
    public IEnumerable<int> Ids { get; set; }
    public IEnumerable<string> Codes { get; set; }
    public MultiEditAction Action { get; set; }
}
