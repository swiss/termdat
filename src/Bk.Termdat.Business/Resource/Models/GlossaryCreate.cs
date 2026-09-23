namespace Bk.Termdat.Business.Resource.Models;

public class GlossaryCreate : IGlossaryModification
{
    public string Code { get; set; }
    public string Description { get; set; }
    public string DescriptionEn { get; set; }
    public string DescriptionDe { get; set; }
    public string DescriptionFr { get; set; }
    public string DescriptionIt { get; set; }
    public string DescriptionRm { get; set; }
}
