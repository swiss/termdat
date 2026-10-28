using System.Collections.Generic;

namespace Bk.Termdat.Business.Resource.Models;

public interface IUserAccountModification
{
    int Id { get; set; }
    string? Information { get; set; }
    IEnumerable<int> AuthorizedCollectionIds { get; set; }
    IEnumerable<int> AuthorizedGlossaryIds { get; set; }
}
