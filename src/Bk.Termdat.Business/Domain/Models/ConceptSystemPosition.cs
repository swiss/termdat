using System.Collections.Generic;

namespace Bk.Termdat.Business.Domain.Models;

public class ConceptSystemPosition : Entity
{
    public string Value { get; set; }
    public int SortIndex { get; set; }
    public int ConceptSystemId { get; set; }

    public ICollection<Entry> Entries { get; set; }
    public ConceptSystem ConceptSystem { get; set; }
}
