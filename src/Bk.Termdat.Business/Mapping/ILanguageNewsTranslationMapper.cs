using System.Collections.Generic;
using System.Threading.Tasks;
using Bk.Termdat.Business.Domain.Models;
using Bk.Termdat.Business.Resource.Models;

namespace Bk.Termdat.Business.Mapping;

public interface ILanguageNewsTranslationMapper
{
    Task<IEnumerable<NewsItemTranslation>> Map(ILanguageNewsTranslation source);
    void MapTranslationsTo(IEnumerable<NewsItemTranslation> translations, ILanguageNewsTranslation destination);
}
