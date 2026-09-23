using System.Collections.Generic;

namespace Bk.Termdat.Business.Domain.Models;

public class Classification : EntityChangeTracking, IMultiLingualEntity<ClassificationTranslation>
{
    public string Code { get; set; }

    public ICollection<ClassificationTranslation> Translations { get; set; }

    public ICollection<Collection> Collections { get; set; }
}
