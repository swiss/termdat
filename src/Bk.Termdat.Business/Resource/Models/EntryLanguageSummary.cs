namespace Bk.Termdat.Business.Resource.Models;

public class EntryLanguageSummary
{
    public int LanguageId { get; set; }
    public int Sequence { get; set; }
    public string Terminus { get; set; }
    public string Name { get; set; }
    public string Phraseology { get; set; }
    public string Abbreviation { get; set; }
    public bool IsObsolete { get; set; }
    public bool IsRare { get; set; }
    public bool ShouldAvoid { get; set; }
}
