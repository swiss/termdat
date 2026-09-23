namespace Bk.Termdat.Business.Domain.Models;

// Subject - Fachgebiet
public class SubjectTranslation : Entity, IEntityNameTranslation
{
    public int LanguageId { get; set; }
    public Language Language { get; set; }
    public string Name { get; set; }
    public Subject Subject { get; set; }
    public int SubjectId { get; set; }
}
