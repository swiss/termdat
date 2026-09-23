using System.Collections.Generic;

namespace Bk.Termdat.Business.Resource.Models;

public class SubjectSummary
{
    public int Id { get; set; }
    public string Name { get; set; }
    public IList<DomainSummary> Domains { get; set; }
}
