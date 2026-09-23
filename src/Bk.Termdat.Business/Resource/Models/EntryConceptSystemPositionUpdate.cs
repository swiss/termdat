using System.Collections.Generic;

namespace Bk.Termdat.Business.Resource.Models;

public class EntryConceptSystemPositionUpdate
{
    public IEnumerable<int> Ids { get; set; }
    public int? ConceptSystemPositionId { get; set; }
    public MultiEditAction Action { get; set; }
}
