using System;
using System.Collections.Generic;

namespace Bk.Termdat.Business.Domain.Models;

public class NewsItem : EntityChangeTracking
{
    public bool IsEnabled { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime ValidTo { get; set; }
    public int SortIndex { get; set; }
    public ICollection<NewsItemTranslation> Translations { get; set; } = [];
}
