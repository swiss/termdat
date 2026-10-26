using System.Collections.Generic;

namespace Bk.Termdat.Business.Resource.Models.Export;

public class WatchlistExportTermdatClassification
{
    public string Subject { get; init; }
    public IList<string> Domains { get; init; } = [];
}
