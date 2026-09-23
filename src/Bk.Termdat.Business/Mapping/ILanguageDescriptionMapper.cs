using System.Collections.Generic;
using System.Threading.Tasks;
using Bk.Termdat.Business.Domain.Models;
using Bk.Termdat.Business.Resource.Models;

namespace Bk.Termdat.Business.Mapping;

public interface ILanguageDescriptionMapper
{
    Task<IEnumerable<T>> Map<T>(ILanguageDescription languageDescription) where T : IEntityNameTranslation, new();
}
