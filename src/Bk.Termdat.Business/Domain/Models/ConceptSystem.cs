using System.Collections.Generic;

namespace Bk.Termdat.Business.Domain.Models;

public class ConceptSystem : EntityChangeTracking
{
    public string Title { get; set; }

    public int CollectionId { get; set; }
    public Collection Collection { get; set; }
    public ICollection<ConceptSystemPosition> ConceptSystemPositions { get; set; }
}
