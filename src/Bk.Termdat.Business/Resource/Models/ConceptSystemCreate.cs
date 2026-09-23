using System.Collections.Generic;

namespace Bk.Termdat.Business.Resource.Models;

public class ConceptSystemCreate : IConceptSystemModification<ConceptSystemPositionCreate>
{
    public string Title { get; set; }
    public int CollectionId { get; set; }
    public IList<ConceptSystemPositionCreate> Positions { get; set; }
}
