namespace Bk.Termdat.Business.Resource.Models;

public class SearchFields
{
    public bool Terminus { get; set; } = true;
    public bool Name { get; set; } = true;
    public bool Abbreviation { get; set; } = true;
    public bool Phraseology { get; set; } = true;
    public bool Definition { get; set; }
    public bool Note { get; set; }
    public bool Context { get; set; }
    public bool Source { get; set; }
    public bool Metadata { get; set; } = true;
    public bool Country { get; set; }
    public bool Comment { get; set; }
}
