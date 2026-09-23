namespace Bk.Termdat.Business.Domain.Models;

public class LanguageTranslation : Entity, IEntityNameTranslation
{
    public string Name { get; set; }

    public string IsoCode { get; set; }

    public int LanguageId { get; set; }

    public Language Language { get; set; }
}
