namespace Bk.Termdat.Business.Resource.Models;

public class LanguageSummary
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int SortIndex { get; set; }
    public bool IsUiLanguage { get; set; }
    public string IsoCode { get; set; }
}
