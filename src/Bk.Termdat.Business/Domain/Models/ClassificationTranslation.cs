namespace Bk.Termdat.Business.Domain.Models;

public class ClassificationTranslation : Entity, IEntityNameTranslation
{
    public string Name { get; set; }

    public int LanguageId { get; set; }

    public int ClassificationId { get; set; }

    public Language Language { get; set; }

    public Classification Classification { get; set; }
}
