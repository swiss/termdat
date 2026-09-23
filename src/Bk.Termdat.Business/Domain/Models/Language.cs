using System.Collections.Generic;

namespace Bk.Termdat.Business.Domain.Models;

public class Language : EntityChangeTracking, IMultiLingualEntity<LanguageTranslation>
{
    public string IsoCode { get; set; }

    public bool IsUiLanguage { get; set; }

    public bool IsSystemLanguage { get; set; }

    public int SortIndex { get; set; }

    public ICollection<LanguageTranslation> Translations { get; set; }
}
