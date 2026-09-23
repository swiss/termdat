namespace Bk.Termdat.Business.Domain.Models;

public class LenochClassificationSubjectDomainMapping
{
    public int Id { get; set; }
    public int LenochClassificationId { get; set; }
    public int SubjectId { get; set; }
    public int? DomainId { get; set; }

    public LenochClassification LenochClassification { get; set; }
    public Subject Subject { get; set; }
    public Domain Domain { get; set; }
}
