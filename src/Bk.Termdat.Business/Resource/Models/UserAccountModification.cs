using System.Collections.Generic;

namespace Bk.Termdat.Business.Resource.Models;

public class UserAccountModification : IUserAccountModification
{
    public int Id { get; set; }
    public string? Information { get; set; }
    public IEnumerable<int> AuthorizedCollectionIds { get; set; }
    public IEnumerable<int> AuthorizedGlossaryIds { get; set; }
}
