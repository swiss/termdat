using Bk.Termdat.Business.Domain.Models;

namespace Bk.Termdat.Business.Domain.Projections;

public class ConceptSystemWithEntryCount
{
    public ConceptSystem ConceptSystem { get; set; }

    public int EntryCount { get; set; }
}
