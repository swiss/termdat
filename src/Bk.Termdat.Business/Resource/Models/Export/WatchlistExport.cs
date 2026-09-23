using System.Collections.Generic;

namespace Bk.Termdat.Business.Resource.Models.Export;

public class WatchlistExport
{
    public string TitlePrefix { get; set; }
    public string TitleSuffix { get; set; }
    public string Date { get; set; }
    public int EntryCount { get; set; }
    public List<WatchlistExportEntry> Entries { get; set; } = new();
}
