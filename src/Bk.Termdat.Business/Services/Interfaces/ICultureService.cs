using System.Collections.Generic;
using Bk.Termdat.Business.Domain.Models;

namespace Bk.Termdat.Business.Services.Interfaces;

public interface ICultureService
{
    string GetCurrentIsoCode();
    string GetDefaultIsoCode();
    string GetTranslation(IEnumerable<IEntityNameTranslation> srcTranslations);
    NewsItemTranslation? GetTranslation(IEnumerable<NewsItemTranslation> translations);

    string GetLanguageTranslation(IEnumerable<LanguageTranslation> srcTranslations);
}
