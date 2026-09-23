namespace Bk.Termdat.Business.Resource.Models;

public class GlossaryDetail : GlossarySummary
{
    public string Description { get; set; }
    public string CreatedBy { get; set; }
    public int EntryCount { get; set; }
}
