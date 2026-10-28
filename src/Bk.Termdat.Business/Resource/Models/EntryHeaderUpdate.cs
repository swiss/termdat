using System.Collections.Generic;

namespace Bk.Termdat.Business.Resource.Models;

public class EntryHeaderUpdate
{
    public IList<int> Ids { get; set; }
    public Status? Status { get; set; }
    public Reliability? Reliability { get; set; }
    public int? OfficeId { get; set; }
    public int? CollectionId { get; set; }
    public IList<int>? SubjectIds { get; set; }
    public IList<int>? DomainIds { get; set; }
    public bool ReplaceSubjects { get; set; }
}
