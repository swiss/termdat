using Bk.Termdat.Business.Domain.Models;

namespace Bk.Termdat.Business.Domain.Projections;

public class GlossaryWithEntryCount
{
    public Glossary Glossary { get; set; }
    public int EntryCount { get; set; }
}
