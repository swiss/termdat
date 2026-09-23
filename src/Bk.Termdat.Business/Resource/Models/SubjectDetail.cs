using System;
using System.Collections.Generic;

namespace Bk.Termdat.Business.Resource.Models;

public class SubjectDetail
{
    public int Id { get; set; }
    public string Name { get; set; }
    public ICollection<DomainDetail> Domains { get; set; }
    public DateTime Created { get; set; }
    public string CreatedBy { get; set; }
    public DateTime Modified { get; set; }
    public string ModifiedBy { get; set; }
    public int EntryCount { get; set; }
}
