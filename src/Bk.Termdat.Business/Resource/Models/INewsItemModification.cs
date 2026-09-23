using System;

namespace Bk.Termdat.Business.Resource.Models;

public interface INewsItemModification : ILanguageNewsTranslation
{
    bool IsEnabled { get; set; }
    DateTime ValidFrom { get; set; }
    DateTime ValidTo { get; set; }
}
