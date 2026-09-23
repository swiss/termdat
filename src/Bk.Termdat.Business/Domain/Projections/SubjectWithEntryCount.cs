using Bk.Termdat.Business.Domain.Models;

namespace Bk.Termdat.Business.Domain.Projections;

public class SubjectWithEntryCount
{
    public Subject Subject { get; set; }
    public int EntryCount { get; set; }
}
