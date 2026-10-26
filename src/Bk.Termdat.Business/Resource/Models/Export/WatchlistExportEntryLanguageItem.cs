using System.Collections.Generic;

namespace Bk.Termdat.Business.Resource.Models.Export;

public class WatchlistExportEntryLanguageItem
{
    public string IsoCode { get; set; }
    public IList<WatchlistExportEntryLanguageDetail> Details { get; set; } = [];
}
