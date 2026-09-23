using System.Collections.Generic;

namespace Bk.Termdat.Business.Resource.Models;

public class EntryContentMove
{
    public IEnumerable<int> Ids { get; set; }
    public IEnumerable<int> LanguageIds { get; set; }
    public string SourceField { get; set; }
    public string DestinationField { get; set; }
    public bool ForAllSynonyms { get; set; }
}

