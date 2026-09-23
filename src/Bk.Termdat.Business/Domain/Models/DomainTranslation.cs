namespace Bk.Termdat.Business.Domain.Models;

// Domain - Sachgebiet
public class DomainTranslation : Entity, IEntityNameTranslation
{
    public int LanguageId { get; set; }
    public Language Language { get; set; }
    public string Name { get; set; }
    public Domain Domain { get; set; }
    public int DomainId { get; set; }
}
