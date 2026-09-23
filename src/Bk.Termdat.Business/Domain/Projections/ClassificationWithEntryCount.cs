using Bk.Termdat.Business.Domain.Models;

namespace Bk.Termdat.Business.Domain.Projections;

public class ClassificationWithEntryCount
{
    public Classification Classification { get; set; }
    public int EntryCount { get; set; }
}
