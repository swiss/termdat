namespace Bk.Termdat.Business.Resource.Models;

public interface IEntryLanguageDetail
{
    int? Id { get; set; }
    int LanguageId { get; set; }
    int Sequence { get; set; }
    string? Terminus { get; set; }
    string? TerminusSource { get; set; }
    string? Name { get; set; }
    string? NameSource { get; set; }
    string? Phraseology { get; set; }
    string? PhraseologySource { get; set; }
    string? Abbreviation { get; set; }
    string? AbbreviationSource { get; set; }
    string? Definition { get; set; }
    string? DefinitionSource { get; set; }
    string? Note { get; set; }
    string? NoteSource { get; set; }
    string? Context { get; set; }
    string? ContextSource { get; set; }
    string? Country { get; set; }
    string? Metadata { get; set; }
    string? MetadataSource { get; set; }
    bool IsObsolete { get; set; }
    bool IsRare { get; set; }
    bool ShouldAvoid { get; set; }

    //Note: Contains the ID provided by the TBX import and originally generated in the Star software.
    string? ExternalSystemId { get; set; }
}
