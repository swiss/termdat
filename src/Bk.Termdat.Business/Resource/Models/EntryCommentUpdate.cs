using System.Collections.Generic;

namespace Bk.Termdat.Business.Resource.Models;

public class EntryCommentUpdate
{
    public IEnumerable<int> Ids { get; set; }
    public string Comment { get; set; }
    public MultiEditAction Action { get; set; }
}
