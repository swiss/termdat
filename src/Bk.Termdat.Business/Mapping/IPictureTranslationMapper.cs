using System.Collections.Generic;
using Bk.Termdat.Business.Domain.Models;
using ResourceModels = Bk.Termdat.Business.Resource.Models;

namespace Bk.Termdat.Business.Mapping;

public interface IPictureTranslationMapper
{
    IEnumerable<PictureTranslation> Map(ResourceModels.PictureDetail pictureDto, IEnumerable<Language> languages);
    PictureTranslation Map(ResourceModels.PictureDetail pictureDto, Language language);
}
