namespace Bk.Termdat.Business.Domain.Models;

public class LenochClassificationTranslation : Entity, IEntityNameTranslation
{
    public string Name { get; set; }

    public int LanguageId { get; set; }

    public int LenochClassificationId { get; set; }

    public Language Language { get; set; }

    public LenochClassification LenochClassification { get; set; }
}
