using System.Collections.Generic;

namespace Bk.Termdat.Business.Resource.Models.Export;

public class WatchlistExportTermdatClassification
{
    public string Subject { get; init; }
    public List<string> Domains { get; init; } = new();
}
