using System.Collections.Generic;

namespace Bk.Termdat.Business.Resource.Models;

public interface IConceptSystemModification<T>
    where T : IConceptSystemPositionModification
{
    string Title { get; set; }
    int CollectionId { get; set; }
    IList<T> Positions { get; set; }
}
