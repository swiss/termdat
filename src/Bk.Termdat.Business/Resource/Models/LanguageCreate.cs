namespace Bk.Termdat.Business.Resource.Models;

public class LanguageCreate : ILanguageModification
{
    public string IsoCode { get; set; }
    public bool IsUiLanguage { get; set; }
    public int SortIndex { get; set; }
    public string DescriptionEn { get; set; }
    public string DescriptionDe { get; set; }
    public string DescriptionFr { get; set; }
    public string DescriptionIt { get; set; }
    public string DescriptionRm { get; set; }
}
