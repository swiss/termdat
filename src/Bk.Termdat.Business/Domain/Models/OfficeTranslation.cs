namespace Bk.Termdat.Business.Domain.Models;

public class OfficeTranslation : Entity, IEntityNameTranslation
{
    public string Name { get; set; }

    public int LanguageId { get; set; }

    public int OfficeId { get; set; }

    public Language Language { get; set; }

    public Office Office { get; set; }
}
