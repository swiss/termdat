using System;
using Bk.Termdat.Business.Domain.Models;

namespace Bk.Termdat.Business.Domain.Projections;

public class IndexEntry
{
    public int EntryId { get; set; }
    public int OfficeId { get; set; }
    public int CollectionId { get; set; }
    public bool CollectionIsActive { get; set; }
    public int[] RelatedCollections { get; set; }
    public int[] LenochClassifications { get; set; }
    public int[] Glossaries { get; set; }
    public int[] References { get; set; }
    public int[] Subjects { get; set; }
    public int[] Domains { get; set; }
    public int[] AdminInfos { get; set; }
    public Status Status { get; set; }
    public Reliability Reliability { get; set; }

    public int LanguageId { get; set; }
    public int SequenceId { get; set; }
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
    public string Comment { get; set; }

    public string CreatedBy { get; set; }
    public DateTime Modified { get; set; }
    public string ModifiedBy { get; set; }
    public bool Deleted { get; set; }
    public int LanguageCount { get; set; }
}
