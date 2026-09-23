using System;

namespace Bk.Termdat.Business.Resource.Models;

public class NewsItemSummary
{
    public int Id { get; set; }
    public bool IsEnabled { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime ValidTo { get; set; }
    public int SortIndex { get; set; }
    public string Title { get; set; }
    public string Text { get; set; }
    public DateTime Created { get; set; }
    public string CreatedBy { get; set; }
    public DateTime Modified { get; set; }
    public string ModifiedBy { get; set; }
    public bool CanMoveUp { get; set; }
    public bool CanMoveDown { get; set; }
}
