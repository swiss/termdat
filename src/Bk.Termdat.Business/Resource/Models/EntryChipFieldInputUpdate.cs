using System.Collections.Generic;

namespace Bk.Termdat.Business.Resource.Models;

public class EntryChipFieldInputUpdate
{
    public IEnumerable<int> Ids { get; set; }
    public IEnumerable<AdminInfoSummary> Codes { get; set; }
    public MultiEditAction Action { get; set; }
}
