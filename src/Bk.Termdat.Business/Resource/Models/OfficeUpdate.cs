namespace Bk.Termdat.Business.Resource.Models;

public class OfficeUpdate : IOfficeModification
{
    public int Id { get; set; }
    public string Code { get; set; }
    public string DescriptionEn { get; set; }
    public string DescriptionDe { get; set; }
    public string DescriptionFr { get; set; }
    public string DescriptionIt { get; set; }
    public string DescriptionRm { get; set; }
}
