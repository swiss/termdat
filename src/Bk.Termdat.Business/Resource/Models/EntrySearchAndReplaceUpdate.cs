using System.Collections.Generic;

namespace Bk.Termdat.Business.Resource.Models;

public class EntrySearchAndReplaceUpdate
{
    public IEnumerable<int> Ids { get; set; }
    public string SearchField { get; set; }
    public string SearchValue { get; set; }
    public string ReplaceValue { get; set; }
    public IEnumerable<int> LanguageIds { get; set; }

    public const string Definition = "definition";
    public const string Note = "note";
    public const string Context = "context";
    public const string Metadata = "metadata";
    public const string Country = "country";
    public const string Comment = "comment";
    public const string Abbreviation = "abbreviation";
    public const string Terminus = "terminus";
    public const string Phraseology = "phraseology";
    public const string Name = "name";
    public const string DefinitionSource = "definitionSource";
    public const string NoteSource = "noteSource";
    public const string ContextSource = "contextSource";
    public const string MetadataSource = "metadataSource";
    public const string TerminusSource = "terminusSource";
    public const string NameSource = "nameSource";
    public const string AbbreviationSource = "abbreviationSource";
    public const string PhraseologySource = "phraseologySource";
}
