namespace Bk.Termdat.Business.Resource.Models;

public class CollectionCreate : ICollectionModification
{
    public string Code { get; set; }
    public int? ClassificationId { get; set; }
    public bool IsObsolete { get; set; }
    public string? Description { get; set; }
    public string? Documentation { get; set; }
    public bool Active { get; set; }
    public bool ActiveApi { get; set; }
    public bool ExportLindas { get; set; }
    public string DescriptionEn { get; set; }
    public string DescriptionDe { get; set; }
    public string DescriptionFr { get; set; }
    public string DescriptionIt { get; set; }
    public string DescriptionRm { get; set; }
}
