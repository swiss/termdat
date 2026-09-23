namespace Bk.Termdat.Business.Domain.Models;

public class PictureTranslation : Entity
{
    public string Source { get; set; }

    public string Description { get; set; }

    public int LanguageId { get; set; }

    public int EntryId { get; set; }

    public Language Language { get; set; }

    public Picture Picture { get; set; }
}
