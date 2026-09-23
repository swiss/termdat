using System.Collections.Generic;

namespace Bk.Termdat.Business.Resource.Models.Export;

public class WatchlistExportEntryLanguageItem
{
    public string IsoCode { get; set; }
    public List<WatchlistExportEntryLanguageDetail> Details { get; set; } = new();
}
