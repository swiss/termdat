namespace Bk.Termdat.Business.Domain.Models;

public class CollectionTranslation : Entity, IEntityNameTranslation
{
    public string Name { get; set; }

    public int LanguageId { get; set; }

    public int CollectionId { get; set; }

    public Language Language { get; set; }

    public Collection Collection { get; set; }
}
