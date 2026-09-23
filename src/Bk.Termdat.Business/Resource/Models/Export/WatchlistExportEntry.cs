using System.Collections.Generic;

namespace Bk.Termdat.Business.Resource.Models.Export;

public class WatchlistExportEntry
{
    public string Value { get; set; }
    public int PositionIndex { get; set; }
    public int EntryId { get; set; }
    public string CreatedKey { get; set; }
    public string Created { get; set; }
    public string CreatedBy { get; set; }
    public string ModifiedKey { get; set; }
    public string Modified { get; set; }
    public string ModifiedBy { get; set; }
    public string Office { get; set; }
    public string Picture { get; set; }
    public string Collection { get; set; }
    public string RelatedCollectionsKey { get; set; }
    public string RelatedCollections { get; set; }
    public string GlossariesKey { get; set; }
    public string Glossaries { get; set; }
    public string AdminInfosKey { get; set; }
    public string AdminInfos { get; set; }
    public string ConceptPositionKey { get; set; }
    public string ConceptPosition { get; set; }
    public string CommentKey { get; set; }
    public string Comment { get; set; }
    public int StatusId { get; set; }
    public string Status { get; set; }
    public string Reliability { get; set; }
    public List<WatchlistExportTermdatClassification> TermdatClassifications { get; set; }
    public List<WatchlistExportEntryLanguageItem> LanguageItems { get; set; } = new();
}
