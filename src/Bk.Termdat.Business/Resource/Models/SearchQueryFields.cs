namespace Bk.Termdat.Business.Resource.Models;

public class SearchQueryFields
{
    public bool Term { get; set; }
    public bool Name { get; set; }
    public bool Abbreviation { get; set; }
    public bool Phraseology { get; set; }
    public bool Definition { get; set; }
    public bool Note { get; set; }
    public bool Context { get; set; }
    public bool Source { get; set; }
    public bool Metadata { get; set; }
    public bool Country { get; set; }
    public bool Comment { get; set; }
}
