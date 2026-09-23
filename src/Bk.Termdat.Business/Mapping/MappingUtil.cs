using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Bk.Termdat.Business.Domain.Models;

namespace Bk.Termdat.Business.Mapping;

public static class MappingUtil
{
    public static void AssertTranslationsLanguageNotNull(IEnumerable<IEntityTranslation> srcTranslations)
    {
        if (srcTranslations == null || srcTranslations.Any(t => t.Language is null))
        {
            throw new AutoMapperMappingException("Cannot map translation without provided Language");
        }
    }
}
