namespace Bk.Termdat.Business.Resource.Models;

public interface ILanguageModification : ILanguageDescription
{
    string IsoCode { get; set; }
    bool IsUiLanguage { get; set; }
    int SortIndex { get; set; }
}
