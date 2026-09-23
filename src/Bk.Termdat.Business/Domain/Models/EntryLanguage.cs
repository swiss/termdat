namespace Bk.Termdat.Business.Domain.Models;

public class EntryLanguage : Entity, IEntityNameTranslation
{
    public int Sequence { get; set; }

    public string Terminus { get; set; }
    public string TerminusSource { get; set; }
    public string Name { get; set; }
    public string NameSource { get; set; }
    public string Abbreviation { get; set; }
    public string AbbreviationSource { get; set; }
    public string Phraseology { get; set; }
    public string PhraseologySource { get; set; }
    public string Definition { get; set; }
    public string DefinitionSource { get; set; }
    public string Note { get; set; }
    public string NoteSource { get; set; }
    public string Context { get; set; }
    public string ContextSource { get; set; }
    public string Country { get; set; }
    public string Metadata { get; set; }
    public string MetadataSource { get; set; }

    public bool IsObsolete { get; set; }
    public bool IsRare { get; set; }
    public bool ShouldAvoid { get; set; }
    public bool IsSynonym => Sequence != 1;

    public int LanguageId { get; set; }
    public int EntryId { get; set; }

    public Language Language { get; set; }
    public Entry Entry { get; set; }

    //Note: Contains the ID provided by the TBX import and originally generated in the Star software.
    public string ExternalSystemId { get; set; }
}
