namespace Bk.Termdat.Business.Resource.Models;

public class CollectionSummary
{
    public int Id { get; set; }
    public string Code { get; set; }
    public string Name { get; set; }
    public string Classification { get; set; }
    public bool IsObsolete { get; set; }
    public bool Deleted { set; get; }
    public bool Active { set; get; }
    public bool ActiveApi { get; set; }
    public bool ExportLindas { get; set; }
}
