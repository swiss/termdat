using System.Collections.Generic;

namespace Bk.Termdat.Business.Resource.Models;

public class EntrySourceUpdate
{
    public IEnumerable<int> Ids { get; set; }
    public IEnumerable<int> LanguageIds { get; set; }
    public IEnumerable<string> TargetFields { get; set; }
    public string UpdateValue { get; set; }
    public MultiEditAction Action { get; set; }
}
