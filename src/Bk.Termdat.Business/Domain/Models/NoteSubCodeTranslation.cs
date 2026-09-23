namespace Bk.Termdat.Business.Domain.Models;

public class NoteSubCodeTranslation : Entity, IEntityNameTranslation
{
    public string Name { get; set; }

    public int LanguageId { get; set; }

    public int NoteSubCodeId { get; set; }

    public Language Language { get; set; }

    public NoteSubCode NoteSubCode { get; set; }
}
