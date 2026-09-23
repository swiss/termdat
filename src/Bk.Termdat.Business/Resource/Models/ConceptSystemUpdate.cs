using System.Collections.Generic;

namespace Bk.Termdat.Business.Resource.Models;

public class ConceptSystemUpdate : IConceptSystemModification<ConceptSystemPositionUpdate>
{
    public int Id { get; set; }
    public string Title { get; set; }
    public int CollectionId { get; set; }
    public IList<ConceptSystemPositionUpdate> Positions { get; set; }
}
