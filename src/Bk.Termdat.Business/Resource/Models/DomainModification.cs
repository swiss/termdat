namespace Bk.Termdat.Business.Resource.Models;

public class DomainModification : ILanguageDescription
{
    public int? Id { get; set; }
    public int SubjectId { get; set; }
    public string DescriptionEn { get; set; }
    public string DescriptionDe { get; set; }
    public string DescriptionFr { get; set; }
    public string DescriptionIt { get; set; }
    public string DescriptionRm { get; set; }
}
