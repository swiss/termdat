namespace Bk.Termdat.Business.Public.Api.Resource.Models;

/// <summary>
/// Field selection
/// </summary>
public class FieldSelection
{
    /// <summary>
    /// Search in term field. Default = true
    /// </summary>
    public bool Terminus { get; set; } = true;

    /// <summary>
    /// Search in name field. Default = true
    /// </summary>
    public bool Name { get; set; } = true;

    /// <summary>
    /// Search in abbreviation field. Default = true
    /// </summary>
    public bool Abbreviation { get; set; } = true;

    /// <summary>
    /// Search in phraseology field. Default = true
    /// </summary>
    public bool Phraseology { get; set; } = true;

    /// <summary>
    /// Search in definition field. Default = false
    /// </summary>
    public bool Definition { get; set; }

    /// <summary>
    /// Search in note field. Default = false
    /// </summary>
    public bool Note { get; set; }

    /// <summary>
    /// Search in context field. Default = false
    /// </summary>
    public bool Context { get; set; }

    /// <summary>
    /// Search in source fields (this includes TerminusSource, NameSource, AbbreviationSource, PhraseologySource, DefinitionSource, NoteSource, ContextSource and MetadataSource). Default = false
    /// </summary>
    public bool Source { get; set; }

    /// <summary>
    /// Search in metadata field. Default = false
    /// </summary>
    public bool Metadata { get; set; }

    /// <summary>
    /// Search in country field. Default = false
    /// </summary>
    public bool Country { get; set; }
}
