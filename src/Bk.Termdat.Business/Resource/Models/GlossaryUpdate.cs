using System;

namespace Bk.Termdat.Business.Resource.Models;

public class GlossaryUpdate : IGlossaryModification
{
    public int Id { get; set; }
    public string Code { get; set; }
    public string Description { get; set; }
    public string DescriptionEn { get; set; }
    public string DescriptionDe { get; set; }
    public string DescriptionFr { get; set; }
    public string DescriptionIt { get; set; }
    public string DescriptionRm { get; set; }
    public DateTime Created { get; set; }
    public string CreatedBy { get; set; }
    public DateTime Modified { get; set; }
    public string ModifiedBy { get; set; }
}
