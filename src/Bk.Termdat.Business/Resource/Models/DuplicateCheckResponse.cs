using System.Collections.Generic;

namespace Bk.Termdat.Business.Resource.Models;

public class DuplicateCheckResponse
{
    public IList<int> Ids { get; set; } = new List<int>();
}
