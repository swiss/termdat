using System.Collections.Generic;

namespace Bk.Termdat.Business.Public.Api.Resource.Models;

/// <summary>
/// Represents a language detail (a.k.a synonym) of an entry.
/// </summary>
public class EntryLanguageDetail
{
    /// <summary>
    /// Id of the language detail.
    /// </summary>
    public int Id { get; set; }
    /// <summary>
    /// Language ISO-Code of this detail.
    /// </summary>
    public string LanguageIsoCode { get; set; }
    /// <summary>
    /// Sequence number of the detail. Starts with 1 and grows sequentially within the same language. 1 is considered the preferred synonym. 
    /// </summary>
    public int Sequence { get; set; }
    /// <summary>
    /// Field terminus.
    /// </summary>
    public string Terminus { get; set; }
    /// <summary>
    /// Field source of terminus. Can contain markdown links, that are shown as links in the TERMDAT 3.0 UI. [Text](https://url).
    /// </summary>
    /// <remarks>
    /// sdafadsf
    /// </remarks>
    public string TerminusSource { get; set; }
    /// <summary>
    /// Field name.
    /// </summary>
    public string Name { get; set; }
    /// <summary>
    /// Field source of name. Can contain markdown links, that are shown as links in the TERMDAT 3.0 UI. [Text](https://url).
    /// </summary>
    public string NameSource { get; set; }
    /// <summary>
    /// Field phraseology.
    /// </summary>
    public string Phraseology { get; set; }
    /// <summary>
    /// Field source of phraseology. Can contain markdown links, that are shown as links in the TERMDAT 3.0 UI. [Text](https://url).
    /// </summary>
    public string PhraseologySource { get; set; }
    /// <summary>
    /// Field abbreviation.
    /// </summary>
    public string Abbreviation { get; set; }
    /// <summary>
    /// Field source of abbreviation. Can contain markdown links, that are shown as links in the TERMDAT 3.0 UI. [Text](https://url).
    /// </summary>
    public string AbbreviationSource { get; set; }
    /// <summary>
    /// Field definition.
    /// </summary>
    public string Definition { get; set; }
    /// <summary>
    /// Field source of definition. Can contain markdown links, that are shown as links in the TERMDAT 3.0 UI. [Text](https://url).
    /// </summary>
    public string DefinitionSource { get; set; }
    /// <summary>
    /// Field note.
    /// </summary>
    public string Note { get; set; }
    /// <summary>
    /// Field source of note. Can contain markdown links, that are shown as links in the TERMDAT 3.0 UI. [Text](https://url).
    /// </summary>
    public string NoteSource { get; set; }
    /// <summary>
    /// Field context.
    /// </summary>
    public string Context { get; set; }
    /// <summary>
    /// Field source of context. Can contain markdown links, that are shown as links in the TERMDAT 3.0 UI. [Text](https://url).
    /// </summary>
    public string ContextSource { get; set; }
    /// <summary>
    /// Field country.
    /// </summary>
    public string Country { get; set; }
    /// <summary>
    /// Field metadata.
    /// </summary>
    public string Metadata { get; set; }
    /// <summary>
    /// Field source of metadata. Can contain markdown links, that are shown as links in the TERMDAT 3.0 UI. [Text](https://url).
    /// </summary>
    public string MetadataSource { get; set; }
    /// <summary>
    /// References from this entry to a entry that contains a language detail in the same language.
    /// </summary>
    public IList<EntryReferenceSummary> References { get; set; }
}
