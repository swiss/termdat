using System.Collections.Generic;

namespace Bk.Termdat.Business.Resource.Models;

public class SubjectModification : ILanguageDescription
{
    public int? Id { get; set; }
    public string DescriptionDe { get; set; }
    public string DescriptionFr { get; set; }
    public string DescriptionEn { get; set; }
    public string DescriptionRm { get; set; }
    public string DescriptionIt { get; set; }
    public IList<DomainModification> Domains { get; set; }
}
