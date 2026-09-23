using System.Collections.Generic;

namespace Bk.Termdat.Business.Domain.Models;

public interface IMultiLingualEntity<TTranslation>
    where TTranslation : class, IEntityTranslation
{
    ICollection<TTranslation> Translations { get; set; }
}
