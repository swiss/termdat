namespace Bk.Termdat.Business.Domain.Models;

public class GlossaryTranslation : Entity, IEntityNameTranslation
{
    public string Name { get; set; }

    public int LanguageId { get; set; }

    public int GlossaryId { get; set; }

    public Language Language { get; set; }

    public Glossary Glossary { get; set; }

}
